using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Services;

namespace BikeShowroomAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BranchDTO>>> GetBranches([FromQuery] int? companyId = null)
    {
        return Ok(await _branchService.GetBranchesAsync(companyId));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BranchDTO>> GetBranch(int id)
    {
        var branch = await _branchService.GetBranchAsync(id);
        return branch == null ? NotFound() : Ok(branch);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<ActionResult<BranchDTO>> CreateBranch(CreateBranchDTO createDto)
    {
        var result = await _branchService.CreateBranchAsync(createDto);
        if (!result.Success)
            return BadRequest(new { message = result.Error });

        return CreatedAtAction(nameof(GetBranch), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<IActionResult> UpdateBranch(int id, CreateBranchDTO updateDto)
    {
        var result = await _branchService.UpdateBranchAsync(id, updateDto);
        return result.Success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<IActionResult> DeleteBranch(int id)
    {
        var result = await _branchService.DeleteBranchAsync(id);
        return result.Success ? NoContent() : NotFound();
    }
}
