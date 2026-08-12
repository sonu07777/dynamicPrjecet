using Microsoft.EntityFrameworkCore;
using BikeShowroomAPI.Data;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models;

namespace BikeShowroomAPI.Services;

public class CompanyService : ICompanyService
{
    private readonly BikeShowroomContext _context;

    public CompanyService(BikeShowroomContext context)
    {
        _context = context;
    }

    public async Task<List<CompanyDTO>> GetCompaniesAsync()
    {
        return await _context.Companies
            .Select(c => ToDto(c))
            .ToListAsync();
    }

    public async Task<CompanyDTO?> GetCompanyAsync(int id)
    {
        var company = await _context.Companies.FindAsync(id);
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

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        return ServiceResult<CompanyDTO>.Ok(ToDto(company));
    }

    public async Task<ServiceResult> UpdateCompanyAsync(int id, CreateCompanyDTO updateDto)
    {
        var company = await _context.Companies.FindAsync(id);
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

        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteCompanyAsync(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null)
            return ServiceResult.Fail("Company not found", ServiceResultStatus.NotFound);

        company.IsActive = false;
        company.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

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
