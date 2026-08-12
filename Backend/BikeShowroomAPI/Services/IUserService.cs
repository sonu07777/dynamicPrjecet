using System.Security.Claims;
using BikeShowroomAPI.DTOs;

namespace BikeShowroomAPI.Services;

public interface IUserService
{
    Task<List<UserDTO>> GetUsersAsync(ClaimsPrincipal caller, int? companyId = null);
    Task<UserDTO?> GetUserAsync(ClaimsPrincipal caller, string id);
    Task<ServiceResult<UserDTO>> CreateUserAsync(ClaimsPrincipal caller, RegisterDTO registerDto);
    Task<ServiceResult> UpdateUserRoleAsync(ClaimsPrincipal caller, string id, UpdateRoleDTO roleDto);
    Task<ServiceResult> ToggleActiveAsync(ClaimsPrincipal caller, string id);
    Task<ServiceResult> UpdateUserAsync(ClaimsPrincipal caller, string id, RegisterDTO updateDto);
    Task<ServiceResult> ResetPasswordAsync(ClaimsPrincipal caller, string id, AdminResetPasswordDTO resetDto);
    Task<List<string>> GetRolesAsync();
}
