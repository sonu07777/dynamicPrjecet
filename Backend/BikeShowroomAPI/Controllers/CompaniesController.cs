using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Services;

namespace BikeShowroomAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompaniesController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<IEnumerable<CompanyDTO>>> GetCompanies()
    {
        return Ok(await _companyService.GetCompaniesAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CompanyDTO>> GetCompany(int id)
    {
        var company = await _companyService.GetCompanyAsync(id);
        return company == null ? NotFound() : Ok(company);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<CompanyDTO>> CreateCompany(CreateCompanyDTO createDto)
    {
        var result = await _companyService.CreateCompanyAsync(createDto);
        if (!result.Success)
            return BadRequest(new { message = result.Error });

        return CreatedAtAction(nameof(GetCompany), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateCompany(int id, CreateCompanyDTO updateDto)
    {
        var result = await _companyService.UpdateCompanyAsync(id, updateDto);
        return result.Success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var result = await _companyService.DeleteCompanyAsync(id);
        return result.Success ? NoContent() : NotFound();
    }
}
