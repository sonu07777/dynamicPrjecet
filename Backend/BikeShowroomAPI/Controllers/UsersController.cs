using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Services;

namespace BikeShowroomAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,CompanyAdmin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers([FromQuery] string? companyId = null)
    {
        return Ok(await _userService.GetUsersAsync(User, companyId));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDTO>> GetUser(string id)
    {
        var user = await _userService.GetUserAsync(User, id);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDTO>> CreateUser(RegisterDTO registerDto)
    {
        var result = await _userService.CreateUserAsync(User, registerDto);
        if (!result.Success)
            return BadRequest(new { message = result.Error });

        return Ok(result.Data);
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UpdateRoleDTO roleDto)
    {
        var result = await _userService.UpdateUserRoleAsync(User, id, roleDto);
        return result.Success ? NoContent() : NotFound();
    }

    [HttpPut("{id}/toggle-active")]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var result = await _userService.ToggleActiveAsync(User, id);
        return result.Success ? NoContent() : NotFound();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, RegisterDTO updateDto)
    {
        var result = await _userService.UpdateUserAsync(User, id, updateDto);
        return result.Success ? NoContent() : NotFound();
    }

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(string id, [FromBody] AdminResetPasswordDTO resetDto)
    {
        var result = await _userService.ResetPasswordAsync(User, id, resetDto);
        return result.Status switch
        {
            ServiceResultStatus.NotFound => NotFound(),
            ServiceResultStatus.Forbidden => Forbid(),
            _ => result.Success ? NoContent() : BadRequest(new { message = result.Error })
        };
    }

    [HttpGet("roles")]
    public async Task<ActionResult<IEnumerable<string>>> GetRoles()
    {
        return Ok(await _userService.GetRolesAsync());
    }
}
