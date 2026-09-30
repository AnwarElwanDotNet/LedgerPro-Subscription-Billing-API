
using BL.Dtos;

namespace BL.Contract
{
    public interface IUserService
    {
        Task<UserResultDto> RegisterAsync(UserDto registerDto);

        Task<UserResultDto> LoginAsync(LoginDto loginDto);

        Task LogoutAsync();

        Task<UserDto?> GetUserByIdAsync(string userId);

        Task<UserDto?> GetUserByEmailAsync(string email);

        Task<(bool Success, string Message)> UpdateUserProfileAsync(EditProfileDto model);

        Task<(bool Success, string Message)> ChangeEmailAsync(ChangeEmailDto model);

        Task<(bool Success, string Message)> ChangePasswordAsync(ChangePasswordDto model);

        Task<(bool Success, string? Token)> ForgotPasswordAsync(string email);

        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto model);

        Guid GetLoggedInUser();
    }
}

