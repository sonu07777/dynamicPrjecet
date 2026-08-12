using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BikeShowroomAPI.Data;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models;

namespace BikeShowroomAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly BikeShowroomContext _context;

    public SuppliersController(BikeShowroomContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDTO>>> GetSuppliers([FromQuery] int? companyId = null)
    {
        var query = _context.Suppliers.AsQueryable();

        if (companyId.HasValue)
            query = query.Where(s => s.CompanyId == companyId.Value);

        var suppliers = await query
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new SupplierDTO
            {
                Id = s.Id,
                CompanyId = s.CompanyId,
                Name = s.Name,
                ContactPerson = s.ContactPerson,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                City = s.City,
                State = s.State,
                Country = s.Country,
                TaxNumber = s.TaxNumber,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return Ok(suppliers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierDTO>> GetSupplier(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null)
            return NotFound();

        return Ok(new SupplierDTO
        {
            Id = supplier.Id,
            CompanyId = supplier.CompanyId,
            Name = supplier.Name,
            ContactPerson = supplier.ContactPerson,
            Email = supplier.Email,
            Phone = supplier.Phone,
            Address = supplier.Address,
            City = supplier.City,
            State = supplier.State,
            Country = supplier.Country,
            TaxNumber = supplier.TaxNumber,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt
        });
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<ActionResult<SupplierDTO>> CreateSupplier(CreateSupplierDTO createDto)
    {
        var supplier = new Supplier
        {
            CompanyId = createDto.CompanyId,
            Name = createDto.Name,
            ContactPerson = createDto.ContactPerson,
            Email = createDto.Email,
            Phone = createDto.Phone,
            Address = createDto.Address,
            City = createDto.City,
            State = createDto.State,
            Country = createDto.Country,
            TaxNumber = createDto.TaxNumber
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, new SupplierDTO
        {
            Id = supplier.Id,
            CompanyId = supplier.CompanyId,
            Name = supplier.Name,
            ContactPerson = supplier.ContactPerson,
            Email = supplier.Email,
            Phone = supplier.Phone,
            Address = supplier.Address,
            City = supplier.City,
            State = supplier.State,
            Country = supplier.Country,
            TaxNumber = supplier.TaxNumber,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<IActionResult> UpdateSupplier(int id, CreateSupplierDTO updateDto)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null)
            return NotFound();

        supplier.Name = updateDto.Name;
        supplier.ContactPerson = updateDto.ContactPerson;
        supplier.Email = updateDto.Email;
        supplier.Phone = updateDto.Phone;
        supplier.Address = updateDto.Address;
        supplier.City = updateDto.City;
        supplier.State = updateDto.State;
        supplier.Country = updateDto.Country;
        supplier.TaxNumber = updateDto.TaxNumber;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null)
            return NotFound();

        supplier.IsActive = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<SupplierDTO>>> SearchSuppliers([FromQuery] string query, [FromQuery] int? companyId = null)
    {
        var suppliersQuery = _context.Suppliers.AsQueryable();

        if (companyId.HasValue)
            suppliersQuery = suppliersQuery.Where(s => s.CompanyId == companyId.Value);

        if (!string.IsNullOrWhiteSpace(query))
        {
            suppliersQuery = suppliersQuery.Where(s =>
                s.Name.Contains(query) ||
                s.ContactPerson.Contains(query) ||
                s.Email.Contains(query) ||
                s.Phone.Contains(query));
        }

        var suppliers = await suppliersQuery
            .Where(s => s.IsActive)
            .Take(50)
            .Select(s => new SupplierDTO
            {
                Id = s.Id,
                CompanyId = s.CompanyId,
                Name = s.Name,
                ContactPerson = s.ContactPerson,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                City = s.City,
                State = s.State,
                Country = s.Country,
                TaxNumber = s.TaxNumber,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return Ok(suppliers);
    }
}
