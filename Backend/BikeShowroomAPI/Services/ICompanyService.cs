using BikeShowroomAPI.DTOs;

namespace BikeShowroomAPI.Services;

public interface ICompanyService
{
    Task<List<CompanyDTO>> GetCompaniesAsync();
    Task<CompanyDTO?> GetCompanyAsync(int id);
    Task<ServiceResult<CompanyDTO>> CreateCompanyAsync(CreateCompanyDTO createDto);
    Task<ServiceResult> UpdateCompanyAsync(int id, CreateCompanyDTO updateDto);
    Task<ServiceResult> DeleteCompanyAsync(int id);
}
