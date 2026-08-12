using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using BikeShowroomAPI.Data;
using BikeShowroomAPI.Models;
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

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly BikeShowroomContext _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        IEmailService emailService,
        BikeShowroomContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _emailService = emailService;
        _context = context;
    }

    public async Task<AuthResponseDTO?> LoginAsync(LoginDTO loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null || !user.IsActive)
            return null;

        // Block login if the user's company or branch has been deactivated.
        if (user.CompanyId.HasValue)
        {
            var companyActive = await _context.Companies
                .AnyAsync(c => c.Id == user.CompanyId.Value && c.IsActive);
            if (!companyActive)
                return null;
        }

        if (user.BranchId.HasValue)
        {
            var branchActive = await _context.Branches
                .AnyAsync(b => b.Id == user.BranchId.Value && b.IsActive);
            if (!branchActive)
                return null;
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded)
            return null;

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);

        return new AuthResponseDTO
        {
            Token = token.Token,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = roles.FirstOrDefault() ?? "User",
            CompanyId = user.CompanyId,
            BranchId = user.BranchId,
            Expiration = token.Expiration
        };
    }

    public async Task<AuthResponseDTO?> RegisterAsync(RegisterDTO registerDto)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
            return null;

        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            PhoneNumber = registerDto.PhoneNumber,
            CompanyId = registerDto.CompanyId,
            BranchId = registerDto.BranchId,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
            return null;

        // Assign role
        if (!string.IsNullOrEmpty(registerDto.Role))
        {
            await _userManager.AddToRoleAsync(user, registerDto.Role);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);

        return new AuthResponseDTO
        {
            Token = token.Token,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = roles.FirstOrDefault() ?? "User",
            CompanyId = user.CompanyId,
            BranchId = user.BranchId,
            Expiration = token.Expiration
        };
    }

    public async Task<UserDTO?> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDTO
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            CompanyId = user.CompanyId,
            BranchId = user.BranchId,
            Role = roles.FirstOrDefault() ?? "User",
            IsActive = user.IsActive
        };
    }

    // Always succeeds silently so callers cannot tell which emails exist.
    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive)
            return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var frontendBase = _configuration["App:FrontendBaseUrl"] ?? "http://localhost:5173";
        var resetUrl = $"{frontendBase}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

        var body = $"""
            <h2 style="font-family:Arial,sans-serif;color:#1f2937;">Reset your password</h2>
            <p style="font-family:Arial,sans-serif;color:#374151;">Hi {WebUtility.HtmlEncode(user.FirstName)},</p>
            <p style="font-family:Arial,sans-serif;color:#374151;">We received a request to reset your password. Click the button below to choose a new one. This link is valid for 24 hours.</p>
            <p style="margin:24px 0;">
              <a href="{WebUtility.HtmlEncode(resetUrl)}"
                 style="background:#f59e0b;color:#ffffff;padding:12px 24px;border-radius:8px;text-decoration:none;font-family:Arial,sans-serif;font-weight:bold;">
                Reset my password
              </a>
            </p>
            <p style="font-family:Arial,sans-serif;color:#6b7280;font-size:13px;">If you didn't request this, you can safely ignore this email.</p>
            """;

        await _emailService.SendAsync(user.Email!, "BikeShowroom - Reset your password", body);
    }

    public async Task<(bool Succeeded, string? Error)> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return (false, "Invalid reset request");

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, "User not found");

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
            return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

        return (true, null);
    }

    private (string Token, DateTime Expiration) GenerateJwtToken(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim("FirstName", user.FirstName),
            new Claim("LastName", user.LastName)
        };

        if (user.CompanyId.HasValue)
            claims.Add(new Claim("CompanyId", user.CompanyId.Value.ToString()));

        if (user.BranchId.HasValue)
            claims.Add(new Claim("BranchId", user.BranchId.Value.ToString()));

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddDays(7);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
    }
}
