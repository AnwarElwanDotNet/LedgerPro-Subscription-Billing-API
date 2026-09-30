using LedgerPro.SubscriptionBilling.Api.Models.Responses;
using LedgerPro.SubscriptionBilling.Api.Service;
using BL.Contract;
using BL.Dtos;
using DAL.UserModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LedgerPro.SubscriptionBilling.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly IUserService _userService;
        private readonly IRefreshToken _refreshTokenService;
        private readonly IRefreshTokenRetriver _refreshTokenRetriver;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(
            TokenService tokenService,
            IUserService userService,
            IRefreshToken refreshTokenService,
            IRefreshTokenRetriver refreshTokenRetriver,
            UserManager<ApplicationUser> userManager)
        {
            _tokenService = tokenService;
            _userService = userService;
            _refreshTokenService = refreshTokenService;
            _refreshTokenRetriver = refreshTokenRetriver;
            _userManager = userManager;
        }


        // ================= REGISTER =================

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] UserDto request)
        {
            var result =
                await _userService.RegisterAsync(request);

            if (!result.Success)
            {
                var errors = result.Errors?
                    .Select(error => new ApiError(
                        ApiErrorCodes.BadRequest,
                        error))
                    .ToList();

                var response =
                    ApiResponse<IEnumerable<string>>.BadRequest(
                        result.Errors != null && result.Errors.Any()
                            ? string.Join(" ", result.Errors)
                            : "Registration failed",
                        errors);

                return BadRequest(response);
            }

            return Ok(
                ApiResponse<bool>.Ok(
                    true,
                    "User registered successfully"));
        }

        // ================= LOGIN =================

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto request)
        {
            if (request == null)
            {
                return Unauthorized(
                    ApiResponse<string>.Unauthorized(
                        "Invalid credentials"));
            }

            var userResult =
                await _userService.LoginAsync(request);

            if (!userResult.Success)
            {
                return Unauthorized(
                    ApiResponse<string>.Unauthorized(
                        "Invalid credentials"));
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return Unauthorized(
                    ApiResponse<string>.Unauthorized(
                        "Invalid credentials"));
            }

            var user =
                await _userManager.FindByEmailAsync(
                    request.Email.Trim().ToLower());

            if (user == null)
            {
                return Unauthorized(
                    ApiResponse<string>.Unauthorized(
                        "Invalid credentials"));
            }

            var claims =
                await BuildUserClaimsAsync(user);

            var accessToken =
                _tokenService.GenerateAccessToken(claims);

            var refreshToken =
                _tokenService.GenerateRefreshToken();

            var storedToken = new RefreshTokenDto
            {
                Token = refreshToken,
                UserId = user.Id.ToString(),
                Expires = DateTime.UtcNow.AddDays(7),
                CurrentState = 1
            };

            var refreshSaved =
                await _refreshTokenService.Refresh(
                    storedToken);

            if (!refreshSaved)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.ServerError(
                        "Unable to create refresh token."));
            }

            SetRefreshTokenCookie(
                refreshToken,
                storedToken.Expires);

            return Ok(
                ApiResponse<object>.Ok(
                    new
                    {
                        AccessToken = accessToken
                    },
                    "Login successful"));
        }

        // ================= REFRESH =================

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (!Request.Cookies.TryGetValue(
                "RefreshToken",
                out var refreshToken))
            {
                return Unauthorized(
                    ApiResponse<string>.Unauthorized(
                        "No refresh token found"));
            }

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return Unauthorized(
                    ApiResponse<string>.Unauthorized(
                        "Invalid or expired refresh token"));
            }

            var storedToken =
                await _refreshTokenRetriver.GetByToken(
                    refreshToken);

            if (storedToken == null ||
                storedToken.Id == Guid.Empty ||
                storedToken.CurrentState != 1 ||
                storedToken.Expires <= DateTime.UtcNow)
            {
                Response.Cookies.Delete("RefreshToken");

                return Unauthorized(
                    ApiResponse<string>.Unauthorized(
                        "Invalid or expired refresh token"));
            }

            if (!Guid.TryParse(
                storedToken.UserId,
                out var userId))
            {
                Response.Cookies.Delete("RefreshToken");

                return Unauthorized(
                    ApiResponse<string>.Unauthorized(
                        "Invalid refresh token"));
            }

            var user =
                await _userManager.FindByIdAsync(
                    userId.ToString());

            if (user == null)
            {
                Response.Cookies.Delete("RefreshToken");

                return Unauthorized(
                    ApiResponse<string>.Unauthorized(
                        "Invalid refresh token"));
            }

            var claims =
                await BuildUserClaimsAsync(user);

            var newAccessToken =
                _tokenService.GenerateAccessToken(claims);

            var newRefreshToken =
                _tokenService.GenerateRefreshToken();

            var newRefreshDto = new RefreshTokenDto
            {
                Token = newRefreshToken,
                UserId = user.Id.ToString(),
                Expires = DateTime.UtcNow.AddDays(7),
                CurrentState = 1
            };

            var refreshSaved =
                await _refreshTokenService.Refresh(
                    newRefreshDto);

            if (!refreshSaved)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<string>.ServerError(
                        "Unable to refresh authentication token."));
            }

            SetRefreshTokenCookie(
                newRefreshToken,
                newRefreshDto.Expires);

            return Ok(
                ApiResponse<object>.Ok(
                    new
                    {
                        AccessToken = newAccessToken
                    },
                    "Token refreshed successfully"));
        }

        // ================= LOGOUT =================

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (Request.Cookies.TryGetValue(
                "RefreshToken",
                out var refreshToken) &&
                !string.IsNullOrWhiteSpace(refreshToken))
            {
                var storedToken =
                    await _refreshTokenRetriver.GetByToken(
                        refreshToken);

                if (storedToken != null &&
                    storedToken.Id != Guid.Empty &&
                    storedToken.CurrentState == 1)
                {
                    await _refreshTokenService.ChangeStatusAsync(
                        storedToken.Id,
                        Domain.Enums.RecordStatus.Deleted);
                }
            }

            Response.Cookies.Delete("RefreshToken");

            return Ok(
                ApiResponse<bool>.Ok(
                    true,
                    "Logged out successfully"));
        }

        // ================= FORGOT PASSWORD =================


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(
            [FromBody] ForgotPasswordDto request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(
                    ApiResponse<bool>.BadRequest(
                        "Email is required.",
                        new[]
                        {
                    new ApiError(
                        ApiErrorCodes.BadRequest,
                        "Email is required.")
                        }));
            }

            var (success, _) =
                  await _userService.ForgotPasswordAsync(
                    request.Email);

            if (!success)
            {
                return BadRequest(
                    ApiResponse<bool>.BadRequest(
                        "Unable to process password reset request.",
                        new[]
                        {
                    new ApiError(
                        ApiErrorCodes.BadRequest,
                        "Unable to process password reset request.")
                        }));
            }

            return Ok(
                ApiResponse<object>.Ok(
                    true,
                     "If the email exists, a password reset link has been sent."));
        }

        // ================= RESET PASSWORD =================

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordDto model)
        {
            var (success, message) =
                await _userService.ResetPasswordAsync(model);

            if (!success)
            {
                return BadRequest(
                    ApiResponse<bool>.BadRequest(
                        message,
                        new[]
                        {
                            new ApiError(
                                ApiErrorCodes.BadRequest,
                                message)
                        }));
            }

            return Ok(
                ApiResponse<bool>.Ok(
                    true,
                    message));
        }

        // ================= ME =================

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId =
                _userService.GetLoggedInUser();

            if (userId == Guid.Empty)
            {
                return Unauthorized(
                    ApiResponse<object>.Unauthorized(
                        "Unauthorized"));
            }

            var user =
                await _userService.GetUserByIdAsync(
                    userId.ToString());

            if (user == null)
            {
                return NotFound(
                    ApiResponse<object>.NotFound(
                        "User not found"));
            }

            return Ok(
                ApiResponse<object>.Ok(user));
        }

        // ================= UPDATE PROFILE =================

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] EditProfileDto model)
        {
            var currentUserId =
                _userService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(
                    ApiResponse<bool>.Unauthorized(
                        "Unauthorized"));
            }

            if (model == null)
            {
                return BadRequest(
                    ApiResponse<bool>.BadRequest(
                        "Invalid request.",
                        new[]
                        {
                            new ApiError(
                                ApiErrorCodes.BadRequest,
                                "Invalid request.")
                        }));
            }

            // Never trust the user ID supplied by the client.
            model.Id = currentUserId;

            var (success, message) =
                await _userService.UpdateUserProfileAsync(
                    model);

            if (!success)
            {
                return BadRequest(
                    ApiResponse<bool>.BadRequest(
                        message,
                        new[]
                        {
                            new ApiError(
                                ApiErrorCodes.BadRequest,
                                message)
                        }));
            }

            return Ok(
                ApiResponse<bool>.Ok(
                    true,
                    message));
        }

        // ================= CHANGE EMAIL =================

        [Authorize]
        [HttpPost("change-email")]
        public async Task<IActionResult> ChangeEmail(
            [FromBody] ChangeEmailDto model)
        {
            var currentUserId =
                _userService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(
                    ApiResponse<bool>.Unauthorized(
                        "Unauthorized"));
            }

            if (model == null)
            {
                return BadRequest(
                    ApiResponse<bool>.BadRequest(
                        "Invalid request.",
                        new[]
                        {
                            new ApiError(
                                ApiErrorCodes.BadRequest,
                                "Invalid request.")
                        }));
            }

            model.Id = currentUserId;

            var (success, message) =
                await _userService.ChangeEmailAsync(
                    model);

            if (!success)
            {
                return BadRequest(
                    ApiResponse<bool>.BadRequest(
                        message,
                        new[]
                        {
                            new ApiError(
                                ApiErrorCodes.BadRequest,
                                message)
                        }));
            }

            return Ok(
                ApiResponse<bool>.Ok(
                    true,
                    message));
        }

        // ================= CHANGE PASSWORD =================

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordDto model)
        {
            var currentUserId =
                _userService.GetLoggedInUser();

            if (currentUserId == Guid.Empty)
            {
                return Unauthorized(
                    ApiResponse<bool>.Unauthorized(
                        "Unauthorized"));
            }

            if (model == null)
            {
                return BadRequest(
                    ApiResponse<bool>.BadRequest(
                        "Invalid request.",
                        new[]
                        {
                            new ApiError(
                                ApiErrorCodes.BadRequest,
                                "Invalid request.")
                        }));
            }

            model.Id = currentUserId;

            var (success, message) =
                await _userService.ChangePasswordAsync(
                    model);

            if (!success)
            {
                return BadRequest(
                    ApiResponse<bool>.BadRequest(
                        message,
                        new[]
                        {
                            new ApiError(
                                ApiErrorCodes.BadRequest,
                                message)
                        }));
            }

            return Ok(
                ApiResponse<bool>.Ok(
                    true,
                    message));
        }

        // ================= PRIVATE HELPERS =================

        private async Task<List<Claim>> BuildUserClaimsAsync(
            ApplicationUser user)
        {
            var roles =
                await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.Email ?? string.Empty)
            };

            claims.AddRange(
                roles.Select(role =>
                    new Claim(
                        ClaimTypes.Role,
                        role)));

            return claims;
        }

        private void SetRefreshTokenCookie(
            string refreshToken,
            DateTime expires)
        {
            Response.Cookies.Append(
                "RefreshToken",
                refreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = expires
                });
        }
    }
}