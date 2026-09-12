using BikeShowroomAPI.DTOs;

namespace BikeShowroomAPI.Services;

public interface IStockTransferService
{
    Task<List<StockTransferDTO>> GetStockTransfersAsync(string? companyId = null, string? fromBranchId = null, string? toBranchId = null, string? status = null);
    Task<StockTransferDTO?> GetStockTransferAsync(string id);
    Task<ServiceResult<StockTransferDTO>> CreateStockTransferAsync(CreateStockTransferDTO createDto, string createdBy);
    Task<ServiceResult> UpdateStockTransferStatusAsync(string id, string status);
    Task<ServiceResult> DeleteStockTransferAsync(string id);
}