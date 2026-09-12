using BikeShowroomAPI.DTOs;

namespace BikeShowroomAPI.Services;

public interface IPurchaseOrderService
{
    Task<List<PurchaseOrderDTO>> GetPurchaseOrdersAsync(string? companyId = null, string? branchId = null, string? status = null);
    Task<PurchaseOrderDTO?> GetPurchaseOrderAsync(string id);
    Task<ServiceResult<PurchaseOrderDTO>> CreatePurchaseOrderAsync(CreatePurchaseOrderDTO createDto, string createdBy);
    Task<ServiceResult> UpdatePurchaseOrderStatusAsync(string id, string status, Dictionary<string, int>? receivedQuantities = null);
    Task<ServiceResult> DeletePurchaseOrderAsync(string id);
}