using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Services;

namespace BikeShowroomAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockTransfersController : ControllerBase
{
    private readonly IStockTransferService _stockTransferService;

    public StockTransfersController(IStockTransferService stockTransferService)
    {
        _stockTransferService = stockTransferService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockTransferDTO>>> GetStockTransfers(
        [FromQuery] string? companyId = null,
        [FromQuery] string? fromBranchId = null,
        [FromQuery] string? toBranchId = null,
        [FromQuery] string? status = null)
    {
        return Ok(await _stockTransferService.GetStockTransfersAsync(companyId, fromBranchId, toBranchId, status));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StockTransferDTO>> GetStockTransfer(string id)
    {
        var transfer = await _stockTransferService.GetStockTransferAsync(id);
        return transfer == null ? NotFound() : Ok(transfer);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
    public async Task<ActionResult<StockTransferDTO>> CreateStockTransfer(CreateStockTransferDTO createDto)
    {
        var createdBy = User.Identity?.Name ?? "Unknown";
        var result = await _stockTransferService.CreateStockTransferAsync(createDto, createdBy);

        if (!result.Success)
            return BadRequest(new { message = result.Error });

        return CreatedAtAction(nameof(GetStockTransfer), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateTransferStatusDTO statusDto)
    {
        var result = await _stockTransferService.UpdateStockTransferStatusAsync(id, statusDto.Status);
        return result.Success ? NoContent() : NotFound(new { message = result.Error });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<IActionResult> DeleteStockTransfer(string id)
    {
        var result = await _stockTransferService.DeleteStockTransferAsync(id);
        return result.Success ? NoContent() : NotFound(new { message = result.Error });
    }
}

public class UpdateTransferStatusDTO
{
    public string Status { get; set; } = string.Empty;
}