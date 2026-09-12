using MongoDB.Driver;
using MongoDB.Bson;
using BikeShowroomAPI.Data.MongoDB;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models.MongoDB;

namespace BikeShowroomAPI.Services.MongoDB;

public class SupplierService : ISupplierService
{
    private readonly IMongoCollection<Supplier> _suppliers;
    private readonly IMongoCollection<Company> _companies;

    public SupplierService(MongoDbContext context)
    {
        _suppliers = context.Suppliers;
        _companies = context.Companies;
    }

    public async Task<List<SupplierDTO>> GetSuppliersAsync(string? companyId = null)
    {
        var filter = Builders<Supplier>.Filter.Eq(s => s.IsActive, true);

        if (!string.IsNullOrEmpty(companyId))
        {
            filter &= Builders<Supplier>.Filter.Eq(s => s.CompanyId, companyId);
        }

        var suppliers = await _suppliers.Find(filter).SortBy(s => s.Name).ToListAsync();
        return suppliers.Select(ToDto).ToList();
    }

    public async Task<SupplierDTO?> GetSupplierAsync(string id)
    {
        var supplier = await _suppliers.Find(s => s.Id == id).FirstOrDefaultAsync();
        return supplier == null ? null : ToDto(supplier);
    }

    public async Task<ServiceResult<SupplierDTO>> CreateSupplierAsync(CreateSupplierDTO createDto)
    {
        var companyExists = await _companies.Find(c => c.Id == createDto.CompanyId && c.IsActive).AnyAsync();
        if (!companyExists)
            return ServiceResult<SupplierDTO>.Fail("Company does not exist or is inactive. Please select a valid company.");

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

        await _suppliers.InsertOneAsync(supplier);
        return ServiceResult<SupplierDTO>.Ok(ToDto(supplier));
    }

    public async Task<ServiceResult> UpdateSupplierAsync(string id, CreateSupplierDTO updateDto)
    {
        var supplier = await _suppliers.Find(s => s.Id == id).FirstOrDefaultAsync();
        if (supplier == null)
            return ServiceResult.Fail("Supplier not found", ServiceResultStatus.NotFound);

        var companyExists = await _companies.Find(c => c.Id == updateDto.CompanyId && c.IsActive).AnyAsync();
        if (!companyExists)
            return ServiceResult.Fail("Company does not exist or is inactive. Please select a valid company.");

        supplier.Name = updateDto.Name;
        supplier.ContactPerson = updateDto.ContactPerson;
        supplier.Email = updateDto.Email;
        supplier.Phone = updateDto.Phone;
        supplier.Address = updateDto.Address;
        supplier.City = updateDto.City;
        supplier.State = updateDto.State;
        supplier.Country = updateDto.Country;
        supplier.TaxNumber = updateDto.TaxNumber;
        supplier.CompanyId = updateDto.CompanyId;

        await _suppliers.ReplaceOneAsync(s => s.Id == id, supplier);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteSupplierAsync(string id)
    {
        var supplier = await _suppliers.Find(s => s.Id == id).FirstOrDefaultAsync();
        if (supplier == null)
            return ServiceResult.Fail("Supplier not found", ServiceResultStatus.NotFound);

        supplier.IsActive = false;
        await _suppliers.ReplaceOneAsync(s => s.Id == id, supplier);

        return ServiceResult.Ok();
    }

    public async Task<List<SupplierDTO>> SearchSuppliersAsync(string query, string? companyId = null)
    {
        var filter = Builders<Supplier>.Filter.Eq(s => s.IsActive, true);

        if (!string.IsNullOrEmpty(companyId))
        {
            filter &= Builders<Supplier>.Filter.Eq(s => s.CompanyId, companyId);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var searchFilter = Builders<Supplier>.Filter.Or(
                Builders<Supplier>.Filter.Regex(s => s.Name, new BsonRegularExpression(query, "i")),
                Builders<Supplier>.Filter.Regex(s => s.ContactPerson, new BsonRegularExpression(query, "i")),
                Builders<Supplier>.Filter.Regex(s => s.Email, new BsonRegularExpression(query, "i")),
                Builders<Supplier>.Filter.Regex(s => s.Phone, new BsonRegularExpression(query, "i"))
            );
            filter &= searchFilter;
        }

        var suppliers = await _suppliers.Find(filter).Limit(50).ToListAsync();
        return suppliers.Select(ToDto).ToList();
    }

    private static SupplierDTO ToDto(Supplier s) => new()
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
    };
}