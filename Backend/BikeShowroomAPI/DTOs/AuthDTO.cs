namespace BikeShowroomAPI.DTOs;

public class LoginDTO
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ForgotPasswordDTO
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDTO
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordDTO
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class AdminResetPasswordDTO
{
    public string NewPassword { get; set; } = string.Empty;
}

public class UpdateRoleDTO
{
    public string Role { get; set; } = string.Empty;
}

public class RegisterDTO
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? CompanyId { get; set; }
    public string? BranchId { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class AuthResponseDTO
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? CompanyId { get; set; }
    public string? BranchId { get; set; }
    public DateTime Expiration { get; set; }
}

public class UserDTO
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? CompanyId { get; set; }
    public string? BranchId { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
