
using BL.Contract;
using BL.Dtos;
using DAL.UserModels;
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace LedgerPro.SubscriptionBilling.Api.Service
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public UserService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IHttpContextAccessor accessor,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = accessor;
            _configuration = configuration;
        }

        // ================= REGISTER =================

        public async Task<UserResultDto> RegisterAsync(UserDto registerDto)
        {
            if (registerDto == null)
                throw new ArgumentNullException(nameof(registerDto));

            var errors = new List<string>();

            registerDto.Email = registerDto.Email?.Trim().ToLower();
            registerDto.Phone = registerDto.Phone?
                .Trim()
                .Replace(" ", "")
                .Replace("-", "");

            if (string.IsNullOrWhiteSpace(registerDto.Email))
                errors.Add("Email is required");

            if (string.IsNullOrWhiteSpace(registerDto.Password))
                errors.Add("Password is required");

            if (string.IsNullOrWhiteSpace(registerDto.Phone))
                errors.Add("Phone is required");

            if (string.IsNullOrWhiteSpace(registerDto.FirstName))
                errors.Add("First name is required");

            if (string.IsNullOrWhiteSpace(registerDto.LastName))
                errors.Add("Last name is required");

            if (errors.Any())
                return new UserResultDto
                {
                    Success = false,
                    Errors = errors
                };

            // ================= PASSWORD VALIDATION =================

            if (registerDto.Password.Length < 8 ||
                !Regex.IsMatch(registerDto.Password, @"[A-Z]") ||
                !Regex.IsMatch(registerDto.Password, @"[a-z]") ||
                !Regex.IsMatch(registerDto.Password, @"[0-9]") ||
                !Regex.IsMatch(registerDto.Password, @"[^a-zA-Z0-9]"))
            {
                return new UserResultDto
                {
                    Success = false,
                    Errors = new[] { "Weak password" }
                };
            }

            // ================= CONFIRM PASSWORD =================

            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return new UserResultDto
                {
                    Success = false,
                    Errors = new[] { "Password mismatch" }
                };
            }

            // ================= EMAIL FORMAT =================

            try
            {
                _ = new MailAddress(registerDto.Email);
            }
            catch
            {
                return new UserResultDto
                {
                    Success = false,
                    Errors = new[] { "Invalid email" }
                };
            }

            // ================= DUPLICATE EMAIL =================

            var existingUser =
                await _userManager.FindByEmailAsync(registerDto.Email);

            if (existingUser != null)
            {
                return new UserResultDto
                {
                    Success = false,
                    Errors = new[] { "Email already exists" }
                };
            }

            // ================= CREATE USER =================

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.Phone,
                EmailConfirmed = true
            };

            var createResult =
                await _userManager.CreateAsync(
                    user,
                    registerDto.Password);

            if (!createResult.Succeeded)
            {
                return new UserResultDto
                {
                    Success = false,
                    Errors = createResult.Errors
                        .Select(x => x.Description)
                };
            }

          

            return new UserResultDto
            {
                Success = true,
                Message = "Registration successful"
            };
        }

        // ================= LOGIN =================

        public async Task<UserResultDto> LoginAsync(LoginDto loginDto)
        {
            var errors = new List<string>();

            if (loginDto == null)
            {
                return new UserResultDto
                {
                    Success = false,
                    Errors = new[] { "Login data is required" }
                };
            }

            loginDto.Email = loginDto.Email?.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(loginDto.Email))
                errors.Add("Email is required");

            if (string.IsNullOrWhiteSpace(loginDto.Password))
                errors.Add("Password is required");

            if (errors.Any())
            {
                return new UserResultDto
                {
                    Success = false,
                    Errors = errors
                };
            }

            var user =
                await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return new UserResultDto
                {
                    Success = false,
                    Errors = new[] { "Invalid credentials" }
                };
            }

            if (!user.EmailConfirmed)
            {
                return new UserResultDto
                {
                    Success = false,
                    Errors = new[] { "Email not confirmed" }
                };
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return new UserResultDto
                {
                    Success = false,
                    Errors = new[]
                    {
                        "Account temporarily locked due to multiple failed attempts. Please try again later."
                    }
                };
            }

            var passwordValid =
                await _userManager.CheckPasswordAsync(
                    user,
                    loginDto.Password);

            if (!passwordValid)
            {
                await _userManager.AccessFailedAsync(user);

                return new UserResultDto
                {
                    Success = false,
                    Errors = new[] { "Invalid credentials" }
                };
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            var roles =
                await _userManager.GetRolesAsync(user);

            return new UserResultDto
            {
                Success = true,
                Message = "Login successful",
                Token = null,
                Errors = null
            };
        }

        // ================= LOGOUT =================

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        // ================= GET USER BY ID =================

        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var user =
                await _userManager.FindByIdAsync(userId);

            if (user == null)
                return null;

            var roles =
                await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Phone = user.PhoneNumber ?? string.Empty,
                OrganizationId = user.OrganizationId,
                Role = roles.FirstOrDefault(),
                Roles = roles.ToList()
            };
        }

        // ================= GET USER BY EMAIL =================

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            email = email.Trim().ToLower();

            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
                return null;

            var roles =
                await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Phone = user.PhoneNumber ?? string.Empty,
                OrganizationId = user.OrganizationId,
                Role = roles.FirstOrDefault(),
                Roles = roles.ToList()
            };
        }

        // ================= UPDATE PROFILE =================

        public async Task<(bool Success, string Message)>
            UpdateUserProfileAsync(EditProfileDto model)
        {
            if (model == null || model.Id == Guid.Empty)
                return (false, "Invalid request.");

            var user =
                await _userManager.FindByIdAsync(
                    model.Id.ToString());

            if (user == null)
                return (false, "Session invalid, please log in again.");

            user.FirstName = model.FirstName.Trim();
            user.LastName = model.LastName.Trim();
            user.PhoneNumber = model.Phone.Trim();

            var result =
                await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return (
                    false,
                    string.Join(
                        " ",
                        result.Errors.Select(e => e.Description)));
            }

            return (true, "Profile updated successfully.");
        }

        // ================= CHANGE EMAIL =================

        public async Task<(bool Success, string Message)>
            ChangeEmailAsync(ChangeEmailDto model)
        {
            if (model == null || model.Id == Guid.Empty)
                return (false, "Invalid user id");

            var user =
                await _userManager.FindByIdAsync(
                    model.Id.ToString());

            if (user == null)
                return (false, "User not found");

            var newEmail =
                model.NewEmail?.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(newEmail))
                return (false, "New email is required");

            try
            {
                _ = new MailAddress(newEmail);
            }
            catch
            {
                return (false, "Invalid email");
            }

            if (user.Email?.ToLower() == newEmail)
                return (
                    false,
                    "New email is the same as current email");

            var existingUser =
                await _userManager.FindByEmailAsync(newEmail);

            if (existingUser != null)
                return (false, "Email already in use");

            user.Email = newEmail;
            user.UserName = newEmail;

            var updateResult =
                await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return (
                    false,
                    string.Join(
                        ", ",
                        updateResult.Errors.Select(e => e.Description)));
            }

            return (true, "Email changed successfully");
        }

        // ================= CHANGE PASSWORD =================

        public async Task<(bool Success, string Message)>
            ChangePasswordAsync(ChangePasswordDto model)
        {
            if (model == null || model.Id == Guid.Empty)
                return (
                    false,
                    "Session invalid, please log in again.");

            var user =
                await _userManager.FindByIdAsync(
                    model.Id.ToString());

            if (user == null)
                return (
                    false,
                    "Session invalid, please log in again.");

            if (string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                string.IsNullOrWhiteSpace(model.NewPassword) ||
                string.IsNullOrWhiteSpace(model.ConfirmNewPassword))
            {
                return (false, "All fields are required.");
            }

            var currentPassword = model.CurrentPassword;
            var newPassword = model.NewPassword;
            var confirmPassword = model.ConfirmNewPassword;

            if (newPassword != confirmPassword)
                return (
                    false,
                    "New password and confirmation do not match.");

            var isCurrentPasswordValid =
                await _userManager.CheckPasswordAsync(
                    user,
                    currentPassword);

            if (!isCurrentPasswordValid)
            {
                await _userManager.AccessFailedAsync(user);

                return (
                    false,
                    "Current password is incorrect.");
            }

            if (await _userManager.CheckPasswordAsync(
                    user,
                    newPassword))
            {
                return (
                    false,
                    "New password must be different from your current password.");
            }

            var result =
                await _userManager.ChangePasswordAsync(
                    user,
                    currentPassword,
                    newPassword);

            if (!result.Succeeded)
            {
                return (
                    false,
                    string.Join(
                        " ",
                        result.Errors.Select(e => e.Description)));
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            return (true, "Password changed successfully.");
        }

        // ================= GET LOGGED-IN USER =================

        public Guid GetLoggedInUser()
        {
            var userId =
                _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirstValue(
                        ClaimTypes.NameIdentifier);

            return Guid.TryParse(
                userId,
                out var id)
                ? id
                : Guid.Empty;
        }

        // ================= FORGOT PASSWORD =================

        public async Task<(bool Success, string? Token)>
            ForgotPasswordAsync(string email)
        {
        

            if (string.IsNullOrWhiteSpace(email))
                return (true, null);

            email = email.Trim().ToLower();

            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
                return (true, null);

            var token =
                await _userManager.GeneratePasswordResetTokenAsync(
                    user);

            return (true, token);
        }

        // ================= RESET PASSWORD =================

        public async Task<(bool Success, string Message)>
            ResetPasswordAsync(ResetPasswordDto model)
        {
            const string genericTokenError =
                "This reset link is invalid or has expired. Please request a new one.";

            if (model == null)
                return (false, "Invalid request.");

            var email =
                model.Email?.Trim().ToLower();

            var token =
                model.Token?.Trim();

            var newPassword = model.NewPassword;
            var confirmPassword = model.ConfirmNewPassword;

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(token) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                return (false, "All fields are required.");
            }

            if (newPassword != confirmPassword)
            {
                return (
                    false,
                    "New password and confirmation do not match.");
            }

            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
                return (false, genericTokenError);

            if (await _userManager.CheckPasswordAsync(
                    user,
                    newPassword))
            {
                return (
                    false,
                    "New password must be different from your current password.");
            }

            var result =
                await _userManager.ResetPasswordAsync(
                    user,
                    token,
                    newPassword);

            if (!result.Succeeded)
            {
                var isTokenError =
                    result.Errors.Any(
                        e => e.Code == "InvalidToken");

                return (
                    false,
                    isTokenError
                        ? genericTokenError
                        : string.Join(
                            " ",
                            result.Errors.Select(
                                e => e.Description)));
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            return (
                true,
                "Password has been reset successfully.");
        }
    }
}

