using System.Security.Claims;
using MongoDB.Driver;
using MongoDB.Bson;
using BikeShowroomAPI.Data.MongoDB;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models.MongoDB;

namespace BikeShowroomAPI.Services.MongoDB;

public class UserService : IUserService
{
    private readonly IMongoCollection<ApplicationUser> _users;
    private readonly IMongoCollection<Company> _companies;
    private readonly IMongoCollection<Branch> _branches;

    public UserService(MongoDbContext context)
    {
        _users = context.Users;
        _companies = context.Companies;
        _branches = context.Branches;
    }

    public async Task<List<UserDTO>> GetUsersAsync(ClaimsPrincipal caller, string? companyId = null)
    {
        var filter = Builders<ApplicationUser>.Filter.Empty;

        var effectiveCompanyId = caller.IsInRole("SuperAdmin") ? companyId : CallerCompanyId(caller);

        if (!string.IsNullOrEmpty(effectiveCompanyId))
        {
            filter &= Builders<ApplicationUser>.Filter.Eq(u => u.CompanyId, effectiveCompanyId);
        }

        var users = await _users.Find(filter).ToListAsync();
        var userDTOs = new List<UserDTO>();

        foreach (var user in users)
        {
            userDTOs.Add(ToDto(user, user.Roles));
        }

        return userDTOs.OrderBy(u => u.Email).ToList();
    }

    public async Task<UserDTO?> GetUserAsync(ClaimsPrincipal caller, string id)
    {
        var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        if (user == null || !CanManage(caller, user))
            return null;

        return ToDto(user, user.Roles);
    }

    public async Task<ServiceResult<UserDTO>> CreateUserAsync(ClaimsPrincipal caller, RegisterDTO registerDto)
    {
        string? companyId = registerDto.CompanyId;
        if (!caller.IsInRole("SuperAdmin"))
        {
            var callerCompanyId = CallerCompanyId(caller);
            if (string.IsNullOrEmpty(callerCompanyId))
                return ServiceResult<UserDTO>.Fail("Your account is not associated with a company.");
            companyId = callerCompanyId;
        }

        var existingUser = await _users.Find(u => u.Email == registerDto.Email).FirstOrDefaultAsync();
        if (existingUser != null)
            return ServiceResult<UserDTO>.Fail("Email already registered");

        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            NormalizedUserName = registerDto.Email.ToUpperInvariant(),
            Email = registerDto.Email,
            NormalizedEmail = registerDto.Email.ToUpperInvariant(),
            EmailConfirmed = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            SecurityStamp = Guid.NewGuid().ToString(),
            PhoneNumber = registerDto.PhoneNumber,
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = false,
            LockoutEnabled = true,
            AccessFailedCount = 0,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            CompanyId = companyId,
            BranchId = registerDto.BranchId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Roles = new List<string>()
        };

        var role = string.IsNullOrWhiteSpace(registerDto.Role) ? "Cashier" : registerDto.Role;
        user.Roles.Add(role);

        await _users.InsertOneAsync(user);

        return ServiceResult<UserDTO>.Ok(ToDto(user, user.Roles));
    }

    public async Task<ServiceResult> UpdateUserRoleAsync(ClaimsPrincipal caller, string id, UpdateRoleDTO roleDto)
    {
        var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        if (user == null || !CanManage(caller, user))
            return ServiceResult.Fail("User not found", ServiceResultStatus.NotFound);

        user.Roles = new List<string> { roleDto.Role };
        await _users.ReplaceOneAsync(u => u.Id == id, user);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ToggleActiveAsync(ClaimsPrincipal caller, string id)
    {
        var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        if (user == null || !CanManage(caller, user))
            return ServiceResult.Fail("User not found", ServiceResultStatus.NotFound);

        user.IsActive = !user.IsActive;
        if (!user.IsActive)
        {
            user.LockoutEnd = DateTimeOffset.MaxValue;
        }
        else
        {
            user.LockoutEnd = null;
        }

        await _users.ReplaceOneAsync(u => u.Id == id, user);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateUserAsync(ClaimsPrincipal caller, string id, RegisterDTO updateDto)
    {
        var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        if (user == null || !CanManage(caller, user))
            return ServiceResult.Fail("User not found", ServiceResultStatus.NotFound);

        user.FirstName = updateDto.FirstName;
        user.LastName = updateDto.LastName;
        user.PhoneNumber = updateDto.PhoneNumber;

        if (caller.IsInRole("SuperAdmin") && !string.IsNullOrEmpty(updateDto.CompanyId))
        {
            user.CompanyId = updateDto.CompanyId;
        }

        user.BranchId = updateDto.BranchId;

        if (!string.IsNullOrWhiteSpace(updateDto.Password) && updateDto.Password.Length >= 6)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateDto.Password);
        }

        await _users.ReplaceOneAsync(u => u.Id == id, user);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> ResetPasswordAsync(ClaimsPrincipal caller, string id, AdminResetPasswordDTO resetDto)
    {
        var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        if (user == null)
            return ServiceResult.Fail("User not found", ServiceResultStatus.NotFound);

        if (!CanManage(caller, user))
            return ServiceResult.Fail("Forbidden", ServiceResultStatus.Forbidden);

        if (string.IsNullOrWhiteSpace(resetDto.NewPassword) || resetDto.NewPassword.Length < 6)
            return ServiceResult.Fail("Password must be at least 6 characters");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetDto.NewPassword);
        await _users.ReplaceOneAsync(u => u.Id == id, user);

        return ServiceResult.Ok();
    }

    public async Task<List<string>> GetRolesAsync()
    {
        return new List<string> { "SuperAdmin", "CompanyAdmin", "BranchManager", "Cashier" };
    }

    private static string? CallerCompanyId(ClaimsPrincipal caller)
        => caller.FindFirst("CompanyId")?.Value;

    private static bool CanManage(ClaimsPrincipal caller, ApplicationUser target)
    {
        if (caller.IsInRole("SuperAdmin"))
            return true;

        var companyId = CallerCompanyId(caller);
        return !string.IsNullOrEmpty(companyId) && target.CompanyId == companyId;
    }

    private static UserDTO ToDto(ApplicationUser user, List<string> roles) => new()
    {
        Id = user.Id,
        Email = user.Email ?? string.Empty,
        FirstName = user.FirstName,
        LastName = user.LastName,
        PhoneNumber = user.PhoneNumber ?? string.Empty,
        CompanyId = user.CompanyId ?? string.Empty,
        BranchId = user.BranchId ?? string.Empty,
        Role = roles.FirstOrDefault() ?? "Cashier",
        IsActive = user.IsActive
    };
}