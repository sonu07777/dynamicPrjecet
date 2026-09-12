using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Services;

namespace BikeShowroomAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _purchaseOrderService;

    public PurchaseOrdersController(IPurchaseOrderService purchaseOrderService)
    {
        _purchaseOrderService = purchaseOrderService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseOrderDTO>>> GetPurchaseOrders(
        [FromQuery] string? companyId = null,
        [FromQuery] string? branchId = null,
        [FromQuery] string? status = null)
    {
        return Ok(await _purchaseOrderService.GetPurchaseOrdersAsync(companyId, branchId, status));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PurchaseOrderDTO>> GetPurchaseOrder(string id)
    {
        var order = await _purchaseOrderService.GetPurchaseOrderAsync(id);
        return order == null ? NotFound() : Ok(order);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
    public async Task<ActionResult<PurchaseOrderDTO>> CreatePurchaseOrder(CreatePurchaseOrderDTO createDto)
    {
        var createdBy = User.Identity?.Name ?? "Unknown";
        var result = await _purchaseOrderService.CreatePurchaseOrderAsync(createDto, createdBy);

        if (!result.Success)
            return BadRequest(new { message = result.Error });

        return CreatedAtAction(nameof(GetPurchaseOrder), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateOrderStatusDTO statusDto)
    {
        var result = await _purchaseOrderService.UpdatePurchaseOrderStatusAsync(id, statusDto.Status, statusDto.ReceivedQuantities);
        return result.Success ? NoContent() : NotFound(new { message = result.Error });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<IActionResult> DeletePurchaseOrder(string id)
    {
        var result = await _purchaseOrderService.DeletePurchaseOrderAsync(id);
        return result.Success ? NoContent() : NotFound(new { message = result.Error });
    }
}

public class UpdateOrderStatusDTO
{
    public string Status { get; set; } = string.Empty;
    public Dictionary<string, int>? ReceivedQuantities { get; set; }
}