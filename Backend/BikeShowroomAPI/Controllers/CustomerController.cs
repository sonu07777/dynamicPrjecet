using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using BikeShowroomAPI.Data.MongoDB;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models.MongoDB;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BikeShowroomAPI.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomerController : ControllerBase
{
	private const int SearchLimit = 50;
	private static readonly EmailAddressAttribute EmailValidator = new();
	private readonly MongoDbContext _context;

	public CustomerController(MongoDbContext context)
	{
		_context = context;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetCustomers(
		[FromQuery] string? companyId = null,
		[FromQuery] string? query = null)
	{
		if (query?.Length > 100)
			return BadRequest(new { message = "Search text cannot exceed 100 characters." });

		var effectiveCompanyId = GetEffectiveCompanyId(companyId);
		if (effectiveCompanyId == null)
			return Forbid();

		var filter = Builders<Customer>.Filter.Eq(customer => customer.IsActive, true);
		if (effectiveCompanyId.Length > 0)
		{
			filter &= Builders<Customer>.Filter.Eq(customer => customer.CompanyId, effectiveCompanyId);
		}

		if (!string.IsNullOrWhiteSpace(query))
		{
			var safeQuery = Regex.Escape(query.Trim());
			var searchFilter = Builders<Customer>.Filter.Or(
				Builders<Customer>.Filter.Regex(customer => customer.FirstName, new BsonRegularExpression(safeQuery, "i")),
				Builders<Customer>.Filter.Regex(customer => customer.LastName, new BsonRegularExpression(safeQuery, "i")),
				Builders<Customer>.Filter.Regex(customer => customer.Email, new BsonRegularExpression(safeQuery, "i")),
				Builders<Customer>.Filter.Regex(customer => customer.Phone, new BsonRegularExpression(safeQuery, "i")));
			filter &= searchFilter;
		}

		var customers = await _context.Customers.Find(filter)
			.SortBy(customer => customer.LastName)
			.ThenBy(customer => customer.FirstName)
			.Limit(SearchLimit)
			.ToListAsync();

		return Ok(customers.Select(ToDto));
	}

	[HttpGet("search")]
	public Task<ActionResult<IEnumerable<CustomerDTO>>> SearchCustomers(
		[FromQuery] string? query = null,
		[FromQuery] string? companyId = null)
		=> GetCustomers(companyId, query);

	[HttpGet("{id}")]
	public async Task<ActionResult<CustomerDTO>> GetCustomer(string id)
	{
		if (!ObjectId.TryParse(id, out _))
			return NotFound();

		var effectiveCompanyId = GetEffectiveCompanyId(null);
		if (effectiveCompanyId == null)
			return Forbid();

		var filter = Builders<Customer>.Filter.Eq(customer => customer.Id, id) &
					 Builders<Customer>.Filter.Eq(customer => customer.IsActive, true);
		if (effectiveCompanyId.Length > 0)
			filter &= Builders<Customer>.Filter.Eq(customer => customer.CompanyId, effectiveCompanyId);

		var customer = await _context.Customers.Find(filter).FirstOrDefaultAsync();
		return customer == null ? NotFound() : Ok(ToDto(customer));
	}

	[HttpPost]
	public async Task<ActionResult<CustomerDTO>> CreateCustomer([FromBody] CreateCustomerDTO createDto)
	{
		if (!ModelState.IsValid || !IsValidCustomer(createDto.FirstName, createDto.LastName, createDto.Email, createDto.Phone, createDto.CreditLimit))
			return BadRequest(new { message = "Provide valid customer details and a non-negative credit limit." });

		var effectiveCompanyId = GetEffectiveCompanyId(createDto.CompanyId);
		if (effectiveCompanyId == null || effectiveCompanyId.Length == 0)
			return effectiveCompanyId == null ? Forbid() : BadRequest(new { message = "A valid company is required." });
		if (!ObjectId.TryParse(effectiveCompanyId, out _))
			return BadRequest(new { message = "A valid company is required." });

		var companyExists = await _context.Companies
			.Find(company => company.Id == effectiveCompanyId && company.IsActive)
			.AnyAsync();
		if (!companyExists)
			return BadRequest(new { message = "Company does not exist or is inactive." });

		var normalizedEmail = createDto.Email.Trim();
		var emailFilter = Builders<Customer>.Filter.Eq(customer => customer.CompanyId, effectiveCompanyId) &
						  Builders<Customer>.Filter.Eq(customer => customer.IsActive, true) &
						  Builders<Customer>.Filter.Regex(
							  customer => customer.Email,
							  new BsonRegularExpression($"^{Regex.Escape(normalizedEmail)}$", "i"));
		var emailExists = await _context.Customers.Find(emailFilter).AnyAsync();
		if (emailExists)
			return Conflict(new { message = "A customer with this email already exists in the company." });

		var customer = new Customer
		{
			CompanyId = effectiveCompanyId,
			FirstName = createDto.FirstName.Trim(),
			LastName = createDto.LastName.Trim(),
			Email = normalizedEmail,
			Phone = createDto.Phone.Trim(),
			Address = CleanOptional(createDto.Address),
			City = CleanOptional(createDto.City),
			State = CleanOptional(createDto.State),
			ZipCode = CleanOptional(createDto.ZipCode),
			CustomerType = string.IsNullOrWhiteSpace(createDto.CustomerType) ? "Regular" : createDto.CustomerType.Trim(),
			CreditLimit = createDto.CreditLimit,
			CurrentBalance = 0,
			IsActive = true,
			CreatedAt = DateTime.UtcNow
		};

		await _context.Customers.InsertOneAsync(customer);
		return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, ToDto(customer));
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateCustomer(string id, [FromBody] CreateCustomerDTO updateDto)
	{
		if (!ObjectId.TryParse(id, out _))
			return NotFound();
		if (!ModelState.IsValid || !IsValidCustomer(updateDto.FirstName, updateDto.LastName, updateDto.Email, updateDto.Phone, updateDto.CreditLimit))
			return BadRequest(new { message = "Provide valid customer details and a non-negative credit limit." });

		var effectiveCompanyId = GetEffectiveCompanyId(null);
		if (effectiveCompanyId == null)
			return Forbid();

		var filter = Builders<Customer>.Filter.Eq(customer => customer.Id, id) &
					 Builders<Customer>.Filter.Eq(customer => customer.IsActive, true);
		if (effectiveCompanyId.Length > 0)
			filter &= Builders<Customer>.Filter.Eq(customer => customer.CompanyId, effectiveCompanyId);

		var customer = await _context.Customers.Find(filter).FirstOrDefaultAsync();
		if (customer == null)
			return NotFound();

		var normalizedEmail = updateDto.Email.Trim();
		var duplicateEmailFilter = Builders<Customer>.Filter.Ne(existing => existing.Id, id) &
								   Builders<Customer>.Filter.Eq(existing => existing.CompanyId, customer.CompanyId) &
								   Builders<Customer>.Filter.Eq(existing => existing.IsActive, true) &
								   Builders<Customer>.Filter.Regex(
									   existing => existing.Email,
									   new BsonRegularExpression($"^{Regex.Escape(normalizedEmail)}$", "i"));
		var duplicateEmail = await _context.Customers.Find(duplicateEmailFilter).AnyAsync();
		if (duplicateEmail)
			return Conflict(new { message = "A customer with this email already exists in the company." });

		customer.FirstName = updateDto.FirstName.Trim();
		customer.LastName = updateDto.LastName.Trim();
		customer.Email = normalizedEmail;
		customer.Phone = updateDto.Phone.Trim();
		customer.Address = CleanOptional(updateDto.Address);
		customer.City = CleanOptional(updateDto.City);
		customer.State = CleanOptional(updateDto.State);
		customer.ZipCode = CleanOptional(updateDto.ZipCode);
		customer.CustomerType = string.IsNullOrWhiteSpace(updateDto.CustomerType) ? "Regular" : updateDto.CustomerType.Trim();
		customer.CreditLimit = updateDto.CreditLimit;

		await _context.Customers.ReplaceOneAsync(filter, customer);
		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteCustomer(string id)
	{
		if (!ObjectId.TryParse(id, out _))
			return NotFound();

		var effectiveCompanyId = GetEffectiveCompanyId(null);
		if (effectiveCompanyId == null)
			return Forbid();

		var filter = Builders<Customer>.Filter.Eq(customer => customer.Id, id) &
					 Builders<Customer>.Filter.Eq(customer => customer.IsActive, true);
		if (effectiveCompanyId.Length > 0)
			filter &= Builders<Customer>.Filter.Eq(customer => customer.CompanyId, effectiveCompanyId);

		var result = await _context.Customers.UpdateOneAsync(
			filter,
			Builders<Customer>.Update.Set(customer => customer.IsActive, false));

		return result.ModifiedCount == 0 ? NotFound() : NoContent();
	}

	private string? GetEffectiveCompanyId(string? requestedCompanyId)
	{
		if (User.IsInRole("SuperAdmin"))
		{
			if (!string.IsNullOrWhiteSpace(requestedCompanyId) && !ObjectId.TryParse(requestedCompanyId, out _))
				return null;
			return requestedCompanyId ?? string.Empty;
		}

		var callerCompanyId = User.FindFirst("CompanyId")?.Value;
		if (string.IsNullOrWhiteSpace(callerCompanyId) ||
			(!string.IsNullOrWhiteSpace(requestedCompanyId) && requestedCompanyId != callerCompanyId))
			return null;

		return callerCompanyId;
	}

	private static bool IsValidCustomer(string firstName, string lastName, string email, string phone, decimal creditLimit)
		=> !string.IsNullOrWhiteSpace(firstName) &&
		   !string.IsNullOrWhiteSpace(lastName) &&
		   EmailValidator.IsValid(email) &&
		   !string.IsNullOrWhiteSpace(phone) &&
		   creditLimit >= 0;

	private static string? CleanOptional(string? value)
		=> string.IsNullOrWhiteSpace(value) ? null : value.Trim();

	private static CustomerDTO ToDto(Customer customer) => new()
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
		TaxNumber = customer.TaxNumber,
		CreditLimit = customer.CreditLimit,
		CurrentBalance = customer.CurrentBalance,
		CustomerType = customer.CustomerType,
		IsActive = customer.IsActive
	};
}
