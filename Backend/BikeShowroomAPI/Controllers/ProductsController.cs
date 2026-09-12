using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Services;

namespace BikeShowroomAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts([FromQuery] string? companyId = null, [FromQuery] string? categoryId = null)
    {
        return Ok(await _productService.GetProductsAsync(companyId, categoryId));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDTO>> GetProduct(string id)
    {
        var product = await _productService.GetProductAsync(id);
        return product == null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
    public async Task<ActionResult<ProductDTO>> CreateProduct(CreateProductDTO createDto)
    {
        var result = await _productService.CreateProductAsync(createDto);
        if (!result.Success)
            return BadRequest(new { message = result.Error });

        return CreatedAtAction(nameof(GetProduct), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin,BranchManager")]
    public async Task<IActionResult> UpdateProduct(string id, CreateProductDTO updateDto)
    {
        var result = await _productService.UpdateProductAsync(id, updateDto);
        if (!result.Success)
            return result.Status == ServiceResultStatus.NotFound ? NotFound() : BadRequest(new { message = result.Error });

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var result = await _productService.DeleteProductAsync(id);
        return result.Success ? NoContent() : NotFound();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> SearchProducts(
        [FromQuery] string? query = null,
        [FromQuery] string? companyId = null)
    {
        return Ok(await _productService.SearchProductsAsync(query, companyId));
    }
}
