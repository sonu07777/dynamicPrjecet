using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using BikeShowroomAPI.Data.MongoDB;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models.MongoDB;
using BikeShowroomAPI.Services;

namespace BikeShowroomAPI.Services.MongoDB;

public class AuthService : IAuthService
{
    private readonly IMongoCollection<ApplicationUser> _users;
    private readonly IMongoCollection<Company> _companies;
    private readonly IMongoCollection<Branch> _branches;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthService(
        MongoDbContext context,
        IConfiguration configuration,
        IEmailService emailService)
    {
        _users = context.Users;
        _companies = context.Companies;
        _branches = context.Branches;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<AuthResponseDTO?> LoginAsync(LoginDTO loginDto)
    {
        var user = await _users.Find(u => u.Email == loginDto.Email && u.IsActive).FirstOrDefaultAsync();
        if (user == null)
            return null;

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash!))
            return null;

        // Check if company is active
        if (!string.IsNullOrEmpty(user.CompanyId))
        {
            var companyActive = await _companies.Find(c => c.Id == user.CompanyId && c.IsActive).AnyAsync();
            if (!companyActive)
                return null;
        }

        // Check if branch is active
        if (!string.IsNullOrEmpty(user.BranchId))
        {
            var branchActive = await _branches.Find(b => b.Id == user.BranchId && b.IsActive).AnyAsync();
            if (!branchActive)
                return null;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _users.ReplaceOneAsync(u => u.Id == user.Id, user);

        var token = GenerateJwtToken(user, user.Roles);

        return new AuthResponseDTO
        {
            Token = token.Token,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Roles.FirstOrDefault() ?? "User",
            CompanyId = user.CompanyId,
            BranchId = user.BranchId,
            Expiration = token.Expiration
        };
    }

    public async Task<AuthResponseDTO?> RegisterAsync(RegisterDTO registerDto)
    {
        var existingUser = await _users.Find(u => u.Email == registerDto.Email).FirstOrDefaultAsync();
        if (existingUser != null)
            return null;

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
            CompanyId = registerDto.CompanyId?.ToString(),
            BranchId = registerDto.BranchId?.ToString(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Roles = new List<string>()
        };

        if (!string.IsNullOrEmpty(registerDto.Role))
        {
            user.Roles.Add(registerDto.Role);
        }

        await _users.InsertOneAsync(user);

        var token = GenerateJwtToken(user, user.Roles);

        return new AuthResponseDTO
        {
            Token = token.Token,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Roles.FirstOrDefault() ?? "User",
            CompanyId = user.CompanyId,
            BranchId = user.BranchId,
            Expiration = token.Expiration
        };
    }

    public async Task<UserDTO?> GetUserByIdAsync(string userId)
    {
        var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null)
            return null;

        return new UserDTO
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            CompanyId = user.CompanyId,
            BranchId = user.BranchId,
            Role = user.Roles.FirstOrDefault() ?? "User",
            IsActive = user.IsActive
        };
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _users.Find(u => u.Email == email && u.IsActive).FirstOrDefaultAsync();
        if (user == null)
            return;

        // Generate a simple reset token (in production, use a more secure approach)
        var token = Guid.NewGuid().ToString();
        var resetTokenExpiry = DateTime.UtcNow.AddHours(24);

        // Store reset token in user document (you might want a separate collection for this)
        user.SecurityStamp = token; // Reusing SecurityStamp for reset token
        await _users.ReplaceOneAsync(u => u.Id == user.Id, user);

        var frontendBase = _configuration["App:FrontendBaseUrl"] ?? "http://localhost:5173";
        var resetUrl = $"{frontendBase}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

        var body = $"""
            <h2 style="font-family:Arial,sans-serif;color:#1f2937;">Reset your password</h2>
            <p style="font-family:Arial,sans-serif;color:#374151;">Hi {System.Net.WebUtility.HtmlEncode(user.FirstName)},</p>
            <p style="font-family:Arial,sans-serif;color:#374151;">We received a request to reset your password. Click the button below to choose a new one. This link is valid for 24 hours.</p>
            <p style="margin:24px 0;">
              <a href="{System.Net.WebUtility.HtmlEncode(resetUrl)}"
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
        var user = await _users.Find(u => u.Email == email && u.SecurityStamp == token).FirstOrDefaultAsync();
        if (user == null)
            return (false, "Invalid reset request");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.SecurityStamp = Guid.NewGuid().ToString(); // Invalidate the reset token
        await _users.ReplaceOneAsync(u => u.Id == user.Id, user);

        return (true, null);
    }

    public async Task<(bool Succeeded, string? Error)> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null)
            return (false, "User not found");

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash!))
            return (false, "Current password is incorrect");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _users.ReplaceOneAsync(u => u.Id == user.Id, user);

        return (true, null);
    }

    private (string Token, DateTime Expiration) GenerateJwtToken(ApplicationUser user, List<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim("FirstName", user.FirstName),
            new Claim("LastName", user.LastName)
        };

        if (!string.IsNullOrEmpty(user.CompanyId))
            claims.Add(new Claim("CompanyId", user.CompanyId));

        if (!string.IsNullOrEmpty(user.BranchId))
            claims.Add(new Claim("BranchId", user.BranchId));

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