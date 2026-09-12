using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Services;

namespace BikeShowroomAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories([FromQuery] string? companyId = null)
    {
        return Ok(await _categoryService.GetCategoriesAsync(companyId));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDTO>> GetCategory(string id)
    {
        var category = await _categoryService.GetCategoryAsync(id);
        return category == null ? NotFound() : Ok(category);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<ActionResult<CategoryDTO>> CreateCategory(CreateCategoryDTO dto)
    {
        var result = await _categoryService.CreateCategoryAsync(dto);
        if (!result.Success)
            return BadRequest(new { message = result.Error });

        return CreatedAtAction(nameof(GetCategory), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<IActionResult> UpdateCategory(string id, CreateCategoryDTO dto)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, dto);
        if (!result.Success)
            return result.Status == ServiceResultStatus.NotFound ? NotFound() : BadRequest(new { message = result.Error });

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<IActionResult> DeleteCategory(string id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        return result.Success ? NoContent() : NotFound();
    }
}
