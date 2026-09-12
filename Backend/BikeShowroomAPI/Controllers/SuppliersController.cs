using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Services;

namespace BikeShowroomAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDTO>>> GetSuppliers([FromQuery] string? companyId = null)
    {
        return Ok(await _supplierService.GetSuppliersAsync(companyId));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierDTO>> GetSupplier(string id)
    {
        var supplier = await _supplierService.GetSupplierAsync(id);
        return supplier == null ? NotFound() : Ok(supplier);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<ActionResult<SupplierDTO>> CreateSupplier(CreateSupplierDTO createDto)
    {
        var result = await _supplierService.CreateSupplierAsync(createDto);
        if (!result.Success)
            return BadRequest(new { message = result.Error });

        return CreatedAtAction(nameof(GetSupplier), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<IActionResult> UpdateSupplier(string id, CreateSupplierDTO updateDto)
    {
        var result = await _supplierService.UpdateSupplierAsync(id, updateDto);
        return result.Success ? NoContent() : NotFound(new { message = result.Error });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<IActionResult> DeleteSupplier(string id)
    {
        var result = await _supplierService.DeleteSupplierAsync(id);
        return result.Success ? NoContent() : NotFound(new { message = result.Error });
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<SupplierDTO>>> SearchSuppliers([FromQuery] string query, [FromQuery] string? companyId = null)
    {
        return Ok(await _supplierService.SearchSuppliersAsync(query, companyId));
    }
}