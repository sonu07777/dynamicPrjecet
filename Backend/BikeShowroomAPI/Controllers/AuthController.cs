using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Services;

namespace BikeShowroomAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDTO>> Login(LoginDTO loginDto)
    {
        var response = await _authService.LoginAsync(loginDto);
        if (response == null)
            return Unauthorized(new { message = "Invalid email or password" });

        return Ok(response);
    }

    [HttpPost("register")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<ActionResult<AuthResponseDTO>> Register(RegisterDTO registerDto)
    {
        var response = await _authService.RegisterAsync(registerDto);
        if (response == null)
            return BadRequest(new { message = "User already exists or registration failed" });

        return Ok(response);
    }

    // Self-service forgot password: emails a reset link. Always returns OK so the
    // response never reveals whether an email is registered.
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO forgotDto)
    {
        await _authService.ForgotPasswordAsync(forgotDto.Email);
        return Ok(new { message = "If that email is registered, a password reset link has been sent." });
    }

    // Completes the reset from the emailed link.
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetDto)
    {
        var result = await _authService.ResetPasswordAsync(resetDto.Email, resetDto.Token, resetDto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { message = result.Error ?? "Password reset failed" });

        return Ok(new { message = "Password has been reset. You can now sign in." });
    }

    // Logged-in user changes their own password.
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordDTO changeDto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var result = await _authService.ChangePasswordAsync(userId, changeDto.CurrentPassword, changeDto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new { message = result.Error ?? "Password change failed" });

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDTO>> GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound();

        return Ok(user);
    }
}
