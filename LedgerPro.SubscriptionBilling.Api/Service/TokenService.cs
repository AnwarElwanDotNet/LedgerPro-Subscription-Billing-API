using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LedgerPro.SubscriptionBilling.Api.Service
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        private const int AccessTokenExpiryMinutes = 15;
        private const int RefreshTokenExpiryDays = 7;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ================= ACCESS TOKEN =================
        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var key = _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(key))
                throw new Exception("JWT Key is missing in appsettings.json");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(AccessTokenExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ================= REFRESH TOKEN =================
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];

            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }

        // ================= HELPERS =================

        public static List<Claim> BuildUserClaims(Guid userId, string email, string role = "User")
        {
            return new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, email ?? string.Empty),
                new Claim(ClaimTypes.Role, role)
            };
        }
    }

}
