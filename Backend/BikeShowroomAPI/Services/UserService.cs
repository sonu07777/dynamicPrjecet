using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BikeShowroomAPI.Data;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models;

namespace BikeShowroomAPI.Services;

/// <summary>
/// User management with company scoping: SuperAdmin can manage anyone,
/// CompanyAdmin can only see/manage users of their own company.
/// The caller's identity (ClaimsPrincipal) is passed in so scoping is enforced server-side.
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly BikeShowroomContext _context;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        BikeShowroomContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<List<UserDTO>> GetUsersAsync(ClaimsPrincipal caller, int? companyId = null)
    {
        var query = _context.Users.AsQueryable();

        // CompanyAdmin is always forced to their own company (ignores the requested filter).
        var effectiveCompanyId = caller.IsInRole("SuperAdmin") ? companyId : CallerCompanyId(caller);

        if (effectiveCompanyId.HasValue)
            query = query.Where(u => u.CompanyId == effectiveCompanyId.Value);

        var users = await query.ToListAsync();
        var userDTOs = new List<UserDTO>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDTOs.Add(ToDto(user, roles));
        }

        return userDTOs.OrderBy(u => u.Email).ToList();
    }

    public async Task<UserDTO?> GetUserAsync(ClaimsPrincipal caller, string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null || !CanManage(caller, user))
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        return ToDto(user, roles);
    }

    public async Task<ServiceResult<UserDTO>> CreateUserAsync(ClaimsPrincipal caller, RegisterDTO registerDto)
    {
        // CompanyAdmin can only create users in their own company.
        int? companyId = registerDto.CompanyId;
        if (!caller.IsInRole("SuperAdmin"))
        {
            var callerCompanyId = CallerCompanyId(caller);
            if (!callerCompanyId.HasValue)
                return ServiceResult<UserDTO>.Fail("Your account is not associated with a company.");
            companyId = callerCompanyId;
        }

        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
            return ServiceResult<UserDTO>.Fail("Email already registered");

        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            PhoneNumber = registerDto.PhoneNumber,
            CompanyId = companyId,
            BranchId = registerDto.BranchId,
            IsActive = true,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, registerDto.Password);
        if (!createResult.Succeeded)
            return ServiceResult<UserDTO>.Fail(string.Join("; ", createResult.Errors.Select(e => e.Description)));

        var role = string.IsNullOrWhiteSpace(registerDto.Role) ? "Cashier" : registerDto.Role;
        await _userManager.AddToRoleAsync(user, role);

        var roles = await _userManager.GetRolesAsync(user);
        return ServiceResult<UserDTO>.Ok(ToDto(user, roles));
    }

    public async Task<ServiceResult> UpdateUserRoleAsync(ClaimsPrincipal caller, string id, UpdateRoleDTO roleDto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null || !CanManage(caller, user))
            return ServiceResult.Fail("User not found", ServiceResultStatus.NotFound);

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, roleDto.Role);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ToggleActiveAsync(ClaimsPrincipal caller, string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null || !CanManage(caller, user))
            return ServiceResult.Fail("User not found", ServiceResultStatus.NotFound);

        user.IsActive = !user.IsActive;
        user.LockoutEnd = user.IsActive ? null : DateTimeOffset.MaxValue;

        await _userManager.UpdateAsync(user);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateUserAsync(ClaimsPrincipal caller, string id, RegisterDTO updateDto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null || !CanManage(caller, user))
            return ServiceResult.Fail("User not found", ServiceResultStatus.NotFound);

        user.FirstName = updateDto.FirstName;
        user.LastName = updateDto.LastName;
        user.PhoneNumber = updateDto.PhoneNumber;
        user.CompanyId = caller.IsInRole("SuperAdmin") ? updateDto.CompanyId : user.CompanyId;
        user.BranchId = updateDto.BranchId;

        if (!string.IsNullOrWhiteSpace(updateDto.Password) && updateDto.Password.Length >= 6)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, updateDto.Password);
        }

        await _userManager.UpdateAsync(user);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ResetPasswordAsync(ClaimsPrincipal caller, string id, AdminResetPasswordDTO resetDto)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return ServiceResult.Fail("User not found", ServiceResultStatus.NotFound);

        if (!CanManage(caller, user))
            return ServiceResult.Fail("Forbidden", ServiceResultStatus.Forbidden);

        if (string.IsNullOrWhiteSpace(resetDto.NewPassword) || resetDto.NewPassword.Length < 6)
            return ServiceResult.Fail("Password must be at least 6 characters");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, resetDto.NewPassword);
        if (!result.Succeeded)
            return ServiceResult.Fail(string.Join("; ", result.Errors.Select(e => e.Description)));

        return ServiceResult.Ok();
    }

    public async Task<List<string>> GetRolesAsync()
    {
        return await _roleManager.Roles
            .Select(r => r.Name!)
            .ToListAsync();
    }

    private static int? CallerCompanyId(ClaimsPrincipal caller)
        => int.TryParse(caller.FindFirst("CompanyId")?.Value, out var id) ? id : null;

    private static bool CanManage(ClaimsPrincipal caller, ApplicationUser target)
    {
        if (caller.IsInRole("SuperAdmin"))
            return true;

        var companyId = CallerCompanyId(caller);
        return companyId.HasValue && target.CompanyId == companyId;
    }

    private static UserDTO ToDto(ApplicationUser user, IList<string> roles) => new()
    {
        Id = user.Id,
        Email = user.Email ?? string.Empty,
        FirstName = user.FirstName,
        LastName = user.LastName,
        PhoneNumber = user.PhoneNumber ?? string.Empty,
        CompanyId = user.CompanyId,
        BranchId = user.BranchId,
        Role = roles.FirstOrDefault() ?? "Cashier",
        IsActive = user.IsActive
    };
}
