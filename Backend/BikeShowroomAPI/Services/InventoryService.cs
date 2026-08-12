using Microsoft.EntityFrameworkCore;
using BikeShowroomAPI.Data;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models;

namespace BikeShowroomAPI.Services;

public class InventoryService : IInventoryService
{
    private readonly BikeShowroomContext _context;

    public InventoryService(BikeShowroomContext context)
    {
        _context = context;
    }

    public async Task<List<InventoryDTO>> GetInventoryAsync(int? branchId = null, int? productId = null)
    {
        var query = _context.Inventories.AsQueryable();

        if (branchId.HasValue)
            query = query.Where(i => i.BranchId == branchId.Value);

        if (productId.HasValue)
            query = query.Where(i => i.ProductId == productId.Value);

        return await query
            .Include(i => i.Product)
            .Include(i => i.Branch)
            .Select(i => ToDto(i))
            .ToListAsync();
    }

    public async Task<InventoryDTO?> GetInventoryItemAsync(int id)
    {
        var inventory = await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Branch)
            .FirstOrDefaultAsync(i => i.Id == id);

        return inventory == null ? null : ToDto(inventory);
    }

    public async Task<ServiceResult<InventoryDTO>> CreateInventoryAsync(Inventory inventory)
    {
        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync();

        return ServiceResult<InventoryDTO>.Ok(new InventoryDTO
        {
            Id = inventory.Id,
            ProductId = inventory.ProductId,
            BranchId = inventory.BranchId,
            Quantity = inventory.Quantity,
            ReservedQuantity = inventory.ReservedQuantity
        });
    }

    public async Task<ServiceResult> UpdateInventoryAsync(int id, Inventory inventory)
    {
        var existingInventory = await _context.Inventories.FindAsync(id);
        if (existingInventory == null)
            return ServiceResult.Fail("Inventory not found", ServiceResultStatus.NotFound);

        existingInventory.Quantity = inventory.Quantity;
        existingInventory.ReservedQuantity = inventory.ReservedQuantity;
        existingInventory.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<int>> AdjustInventoryAsync(InventoryAdjustmentDTO adjustment)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == adjustment.ProductId && i.BranchId == adjustment.BranchId);

        if (inventory == null)
        {
            inventory = new Inventory
            {
                ProductId = adjustment.ProductId,
                BranchId = adjustment.BranchId,
                Quantity = adjustment.Quantity,
                ReservedQuantity = 0
            };
            _context.Inventories.Add(inventory);
        }
        else
        {
            inventory.Quantity += adjustment.Quantity;
            inventory.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return ServiceResult<int>.Ok(inventory.Quantity);
    }

    public async Task<List<InventoryDTO>> GetLowStockAsync(int? branchId = null)
    {
        var query = _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Branch)
            .Where(i => i.Product.MinStockLevel.HasValue && i.Quantity <= i.Product.MinStockLevel.Value);

        if (branchId.HasValue)
            query = query.Where(i => i.BranchId == branchId.Value);

        return await query
            .Select(i => ToDto(i))
            .ToListAsync();
    }

    private static InventoryDTO ToDto(Inventory i) => new()
    {
        Id = i.Id,
        ProductId = i.ProductId,
        BranchId = i.BranchId,
        Quantity = i.Quantity,
        ReservedQuantity = i.ReservedQuantity,
        ProductName = i.Product.Name,
        BranchName = i.Branch.Name
    };
}
