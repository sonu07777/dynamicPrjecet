using MongoDB.Driver;
using MongoDB.Bson;
using BikeShowroomAPI.Data.MongoDB;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models.MongoDB;

namespace BikeShowroomAPI.Services.MongoDB;

public class CompanyService : ICompanyService
{
    private readonly IMongoCollection<Company> _companies;

    public CompanyService(MongoDbContext context)
    {
        _companies = context.Companies;
    }

    public async Task<List<CompanyDTO>> GetCompaniesAsync()
    {
        var companies = await _companies.Find(_ => true).ToListAsync();
        return companies.Select(ToDto).ToList();
    }

    public async Task<CompanyDTO?> GetCompanyAsync(string id)
    {
        var company = await _companies.Find(c => c.Id == id).FirstOrDefaultAsync();
        return company == null ? null : ToDto(company);
    }

    public async Task<ServiceResult<CompanyDTO>> CreateCompanyAsync(CreateCompanyDTO createDto)
    {
        var company = new Company
        {
            Name = createDto.Name,
            Address = createDto.Address,
            City = createDto.City,
            State = createDto.State,
            Country = createDto.Country,
            Phone = createDto.Phone,
            Email = createDto.Email,
            Website = createDto.Website,
            TaxNumber = createDto.TaxNumber
        };

        await _companies.InsertOneAsync(company);
        return ServiceResult<CompanyDTO>.Ok(ToDto(company));
    }

    public async Task<ServiceResult> UpdateCompanyAsync(string id, CreateCompanyDTO updateDto)
    {
        var company = await _companies.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (company == null)
            return ServiceResult.Fail("Company not found", ServiceResultStatus.NotFound);

        company.Name = updateDto.Name;
        company.Address = updateDto.Address;
        company.City = updateDto.City;
        company.State = updateDto.State;
        company.Country = updateDto.Country;
        company.Phone = updateDto.Phone;
        company.Email = updateDto.Email;
        company.Website = updateDto.Website;
        company.TaxNumber = updateDto.TaxNumber;
        company.UpdatedAt = DateTime.UtcNow;

        await _companies.ReplaceOneAsync(c => c.Id == id, company);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteCompanyAsync(string id)
    {
        var company = await _companies.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (company == null)
            return ServiceResult.Fail("Company not found", ServiceResultStatus.NotFound);

        company.IsActive = false;
        company.UpdatedAt = DateTime.UtcNow;
        await _companies.ReplaceOneAsync(c => c.Id == id, company);

        return ServiceResult.Ok();
    }

    private static CompanyDTO ToDto(Company c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Address = c.Address,
        City = c.City,
        State = c.State,
        Country = c.Country,
        Phone = c.Phone,
        Email = c.Email,
        Website = c.Website,
        Logo = c.Logo,
        TaxNumber = c.TaxNumber,
        IsActive = c.IsActive
    };
}