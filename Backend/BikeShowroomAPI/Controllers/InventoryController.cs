using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models;
using BikeShowroomAPI.Services;

namespace BikeShowroomAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryDTO>>> GetInventory([FromQuery] int? branchId = null, [FromQuery] int? productId = null)
    {
        return Ok(await _inventoryService.GetInventoryAsync(branchId, productId));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryDTO>> GetInventoryItem(int id)
    {
        var inventory = await _inventoryService.GetInventoryItemAsync(id);
        return inventory == null ? NotFound() : Ok(inventory);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
    public async Task<ActionResult<InventoryDTO>> CreateInventory(Inventory inventory)
    {
        var result = await _inventoryService.CreateInventoryAsync(inventory);
        if (!result.Success)
            return BadRequest(new { message = result.Error });

        return CreatedAtAction(nameof(GetInventoryItem), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
    public async Task<IActionResult> UpdateInventory(int id, Inventory inventory)
    {
        if (id != inventory.Id)
            return BadRequest();

        var result = await _inventoryService.UpdateInventoryAsync(id, inventory);
        return result.Success ? NoContent() : NotFound();
    }

    [HttpPost("adjust")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
    public async Task<IActionResult> AdjustInventory([FromBody] InventoryAdjustmentDTO adjustment)
    {
        var result = await _inventoryService.AdjustInventoryAsync(adjustment);
        return Ok(new { message = "Inventory adjusted successfully", newQuantity = result.Data });
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<InventoryDTO>>> GetLowStock([FromQuery] int? branchId = null)
    {
        return Ok(await _inventoryService.GetLowStockAsync(branchId));
    }
}
