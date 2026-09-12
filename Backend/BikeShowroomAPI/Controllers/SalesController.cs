using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Services;

namespace BikeShowroomAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly ISalesService _salesService;

    public SalesController(ISalesService salesService)
    {
        _salesService = salesService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SaleDTO>>> GetSales(
        [FromQuery] string? companyId = null,
        [FromQuery] string? branchId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        return Ok(await _salesService.GetSalesAsync(companyId, branchId, status, fromDate, toDate));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SaleDTO>> GetSale(string id)
    {
        var sale = await _salesService.GetSaleAsync(id);
        return sale == null ? NotFound() : Ok(sale);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager,Cashier")]
    public async Task<ActionResult<SaleDTO>> CreateSale(CreateSaleDTO createDto)
    {
        var createdBy = User.Identity?.Name ?? "Unknown";
        var result = await _salesService.CreateSaleAsync(createDto, createdBy);

        if (!result.Success)
            return result.Status == ServiceResultStatus.NotFound ? NotFound(new { message = result.Error }) : BadRequest(new { message = result.Error });

        return CreatedAtAction(nameof(GetSale), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
    public async Task<IActionResult> UpdateSaleStatus(string id, [FromBody] UpdateSaleStatusDTO statusDto)
    {
        var result = await _salesService.UpdateSaleStatusAsync(id, statusDto.Status);
        return result.Success ? NoContent() : NotFound(new { message = result.Error });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<IActionResult> DeleteSale(string id)
    {
        var result = await _salesService.DeleteSaleAsync(id);
        return result.Success ? NoContent() : NotFound(new { message = result.Error });
    }
}

public class UpdateSaleStatusDTO
{
    public string Status { get; set; } = string.Empty;
}