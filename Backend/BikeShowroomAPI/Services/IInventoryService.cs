using BikeShowroomAPI.DTOs;

namespace BikeShowroomAPI.Services;

public interface IInventoryService
{
    Task<List<InventoryDTO>> GetInventoryAsync(string? branchId = null, string? productId = null);
    Task<InventoryDTO?> GetInventoryItemAsync(string id);
    Task<ServiceResult<InventoryDTO>> CreateInventoryAsync(string productId, string branchId, int quantity);
    Task<ServiceResult> UpdateInventoryAsync(string id, int quantity, int reservedQuantity);
    Task<ServiceResult<int>> AdjustInventoryAsync(InventoryAdjustmentDTO adjustment);
    Task<List<InventoryDTO>> GetLowStockAsync(string? branchId = null);
}
