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
public class CustomersController : ControllerBase
{
    private readonly BikeShowroomContext _context;

    public CustomersController(BikeShowroomContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetCustomers([FromQuery] int? companyId = null)
    {
        var query = _context.Customers.AsQueryable();

        if (companyId.HasValue)
            query = query.Where(c => c.CompanyId == companyId.Value);

        var customers = await query
            .Where(c => c.IsActive)
            .Select(c => new CustomerDTO
            {
                Id = c.Id,
                CompanyId = c.CompanyId,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                City = c.City,
                State = c.State,
                ZipCode = c.ZipCode,
                CustomerType = c.CustomerType,
                CreditLimit = c.CreditLimit,
                CurrentBalance = c.CurrentBalance,
                IsActive = c.IsActive
            })
            .ToListAsync();

        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDTO>> GetCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);

        if (customer == null)
            return NotFound();

        var customerDto = new CustomerDTO
        {
            Id = customer.Id,
            CompanyId = customer.CompanyId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            City = customer.City,
            State = customer.State,
            ZipCode = customer.ZipCode,
            CustomerType = customer.CustomerType,
            CreditLimit = customer.CreditLimit,
            CurrentBalance = customer.CurrentBalance,
            IsActive = customer.IsActive
        };

        return Ok(customerDto);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDTO>> CreateCustomer(CreateCustomerDTO createDto)
    {
        var customer = new Customer
        {
            CompanyId = createDto.CompanyId,
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            Email = createDto.Email,
            Phone = createDto.Phone,
            Address = createDto.Address,
            City = createDto.City,
            State = createDto.State,
            ZipCode = createDto.ZipCode,
            CustomerType = createDto.CustomerType,
            CreditLimit = createDto.CreditLimit
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var customerDto = new CustomerDTO
        {
            Id = customer.Id,
            CompanyId = customer.CompanyId,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            City = customer.City,
            State = customer.State,
            ZipCode = customer.ZipCode,
            CustomerType = customer.CustomerType,
            CreditLimit = customer.CreditLimit,
            CurrentBalance = customer.CurrentBalance,
            IsActive = customer.IsActive
        };

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customerDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, CreateCustomerDTO updateDto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return NotFound();

        customer.FirstName = updateDto.FirstName;
        customer.LastName = updateDto.LastName;
        customer.Email = updateDto.Email;
        customer.Phone = updateDto.Phone;
        customer.Address = updateDto.Address;
        customer.City = updateDto.City;
        customer.State = updateDto.State;
        customer.ZipCode = updateDto.ZipCode;
        customer.CustomerType = updateDto.CustomerType;
        customer.CreditLimit = updateDto.CreditLimit;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return NotFound();

        customer.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<CustomerDTO>>> SearchCustomers([FromQuery] string query, [FromQuery] int? companyId = null)
    {
        var customersQuery = _context.Customers.AsQueryable();

        if (companyId.HasValue)
            customersQuery = customersQuery.Where(c => c.CompanyId == companyId.Value);

        if (!string.IsNullOrWhiteSpace(query))
        {
            customersQuery = customersQuery.Where(c =>
                c.FirstName.Contains(query) ||
                c.LastName.Contains(query) ||
                c.Email.Contains(query) ||
                c.Phone.Contains(query));
        }

        var customers = await customersQuery
            .Where(c => c.IsActive)
            .Take(50)
            .Select(c => new CustomerDTO
            {
                Id = c.Id,
                CompanyId = c.CompanyId,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                City = c.City,
                State = c.State,
                ZipCode = c.ZipCode,
                CustomerType = c.CustomerType,
                CreditLimit = c.CreditLimit,
                CurrentBalance = c.CurrentBalance,
                IsActive = c.IsActive
            })
            .ToListAsync();

        return Ok(customers);
    }
}

public class CustomerDTO
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string CustomerType { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCustomerDTO
{
    public int CompanyId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string CustomerType { get; set; } = "Regular";
    public decimal CreditLimit { get; set; }
}
