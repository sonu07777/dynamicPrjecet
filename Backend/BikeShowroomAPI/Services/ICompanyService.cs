using BikeShowroomAPI.DTOs;

namespace BikeShowroomAPI.Services;

public interface ICompanyService
{
    Task<List<CompanyDTO>> GetCompaniesAsync();
    Task<CompanyDTO?> GetCompanyAsync(string id);
    Task<ServiceResult<CompanyDTO>> CreateCompanyAsync(CreateCompanyDTO createDto);
    Task<ServiceResult> UpdateCompanyAsync(string id, CreateCompanyDTO updateDto);
    Task<ServiceResult> DeleteCompanyAsync(string id);
}