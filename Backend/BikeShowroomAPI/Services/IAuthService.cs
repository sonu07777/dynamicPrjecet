using BikeShowroomAPI.DTOs;

namespace BikeShowroomAPI.Services;

public interface IAuthService
{
    Task<AuthResponseDTO?> LoginAsync(LoginDTO loginDto);
    Task<AuthResponseDTO?> RegisterAsync(RegisterDTO registerDto);
    Task<UserDTO?> GetUserByIdAsync(string userId);
    Task ForgotPasswordAsync(string email);
    Task<(bool Succeeded, string? Error)> ResetPasswordAsync(string email, string token, string newPassword);
    Task<(bool Succeeded, string? Error)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
}