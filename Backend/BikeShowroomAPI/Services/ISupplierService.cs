using BikeShowroomAPI.DTOs;

namespace BikeShowroomAPI.Services;

public interface ISupplierService
{
    Task<List<SupplierDTO>> GetSuppliersAsync(string? companyId = null);
    Task<SupplierDTO?> GetSupplierAsync(string id);
    Task<ServiceResult<SupplierDTO>> CreateSupplierAsync(CreateSupplierDTO createDto);
    Task<ServiceResult> UpdateSupplierAsync(string id, CreateSupplierDTO updateDto);
    Task<ServiceResult> DeleteSupplierAsync(string id);
    Task<List<SupplierDTO>> SearchSuppliersAsync(string query, string? companyId = null);
}