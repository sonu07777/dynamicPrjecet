using BikeShowroomAPI.DTOs;

namespace BikeShowroomAPI.Services;

public interface ISalesService
{
    Task<List<SaleDTO>> GetSalesAsync(string? companyId = null, string? branchId = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<SaleDTO?> GetSaleAsync(string id);
    Task<ServiceResult<SaleDTO>> CreateSaleAsync(CreateSaleDTO createDto, string createdBy);
    Task<ServiceResult> UpdateSaleStatusAsync(string id, string status);
    Task<ServiceResult> DeleteSaleAsync(string id);
}