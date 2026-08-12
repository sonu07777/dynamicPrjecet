using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models;

namespace BikeShowroomAPI.Services;

public interface IInventoryService
{
    Task<List<InventoryDTO>> GetInventoryAsync(int? branchId = null, int? productId = null);
    Task<InventoryDTO?> GetInventoryItemAsync(int id);
    Task<ServiceResult<InventoryDTO>> CreateInventoryAsync(Inventory inventory);
    Task<ServiceResult> UpdateInventoryAsync(int id, Inventory inventory);
    Task<ServiceResult<int>> AdjustInventoryAsync(InventoryAdjustmentDTO adjustment);
    Task<List<InventoryDTO>> GetLowStockAsync(int? branchId = null);
}
