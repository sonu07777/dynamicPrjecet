using MongoDB.Driver;
using MongoDB.Bson;
using BikeShowroomAPI.Data.MongoDB;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models.MongoDB;

namespace BikeShowroomAPI.Services.MongoDB;

public class InventoryService : IInventoryService
{
    private readonly IMongoCollection<Inventory> _inventories;
    private readonly IMongoCollection<Product> _products;
    private readonly IMongoCollection<Branch> _branches;

    public InventoryService(MongoDbContext context)
    {
        _inventories = context.Inventories;
        _products = context.Products;
        _branches = context.Branches;
    }

    public async Task<List<InventoryDTO>> GetInventoryAsync(string? branchId = null, string? productId = null)
    {
        var filter = Builders<Inventory>.Filter.Empty;

        if (!string.IsNullOrEmpty(branchId))
        {
            filter &= Builders<Inventory>.Filter.Eq(i => i.BranchId, branchId);
        }

        if (!string.IsNullOrEmpty(productId))
        {
            filter &= Builders<Inventory>.Filter.Eq(i => i.ProductId, productId);
        }

        var inventories = await _inventories.Find(filter).ToListAsync();

        // Get product and branch names
        var productIds = inventories.Select(i => i.ProductId).Distinct().ToList();
        var branchIds = inventories.Select(i => i.BranchId).Distinct().ToList();

        var products = await _products.Find(p => productIds.Contains(p.Id)).ToListAsync();
        var branches = await _branches.Find(b => branchIds.Contains(b.Id)).ToListAsync();

        var productDict = products.ToDictionary(p => p.Id, p => p.Name);
        var branchDict = branches.ToDictionary(b => b.Id, b => b.Name);

        return inventories.Select(i => ToDto(i, productDict, branchDict)).ToList();
    }

    public async Task<InventoryDTO?> GetInventoryItemAsync(string id)
    {
        var inventory = await _inventories.Find(i => i.Id == id).FirstOrDefaultAsync();

        if (inventory == null) return null;

        var product = await _products.Find(p => p.Id == inventory.ProductId).FirstOrDefaultAsync();
        var branch = await _branches.Find(b => b.Id == inventory.BranchId).FirstOrDefaultAsync();

        return ToDto(inventory,
            new Dictionary<string, string> { [inventory.ProductId] = product?.Name ?? "" },
            new Dictionary<string, string> { [inventory.BranchId] = branch?.Name ?? "" });
    }

    public async Task<ServiceResult<InventoryDTO>> CreateInventoryAsync(string productId, string branchId, int quantity)
    {
        var productExists = await _products.Find(p => p.Id == productId).AnyAsync();
        if (!productExists)
            return ServiceResult<InventoryDTO>.Fail("Product not found", ServiceResultStatus.NotFound);

        var branchExists = await _branches.Find(b => b.Id == branchId).AnyAsync();
        if (!branchExists)
            return ServiceResult<InventoryDTO>.Fail("Branch not found", ServiceResultStatus.NotFound);

        // Check if inventory already exists
        var existingFilter = Builders<Inventory>.Filter.And(
            Builders<Inventory>.Filter.Eq(i => i.ProductId, productId),
            Builders<Inventory>.Filter.Eq(i => i.BranchId, branchId));

        var existing = await _inventories.Find(existingFilter).FirstOrDefaultAsync();
        if (existing != null)
            return ServiceResult<InventoryDTO>.Fail("Inventory already exists for this product and branch", ServiceResultStatus.BadRequest);

        var inventory = new Inventory
        {
            ProductId = productId,
            BranchId = branchId,
            Quantity = quantity,
            ReservedQuantity = 0
        };

        await _inventories.InsertOneAsync(inventory);

        var product = await _products.Find(p => p.Id == productId).FirstOrDefaultAsync();
        var branch = await _branches.Find(b => b.Id == branchId).FirstOrDefaultAsync();

        return ServiceResult<InventoryDTO>.Ok(ToDto(inventory,
            new Dictionary<string, string> { [productId] = product?.Name ?? "" },
            new Dictionary<string, string> { [branchId] = branch?.Name ?? "" }));
    }

    public async Task<ServiceResult> UpdateInventoryAsync(string id, int quantity, int reservedQuantity)
    {
        var inventory = await _inventories.Find(i => i.Id == id).FirstOrDefaultAsync();
        if (inventory == null)
            return ServiceResult.Fail("Inventory not found", ServiceResultStatus.NotFound);

        inventory.Quantity = quantity;
        inventory.ReservedQuantity = reservedQuantity;
        inventory.UpdatedAt = DateTime.UtcNow;

        await _inventories.ReplaceOneAsync(i => i.Id == id, inventory);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<int>> AdjustInventoryAsync(InventoryAdjustmentDTO adjustment)
    {
        var filter = Builders<Inventory>.Filter.And(
            Builders<Inventory>.Filter.Eq(i => i.ProductId, adjustment.ProductId),
            Builders<Inventory>.Filter.Eq(i => i.BranchId, adjustment.BranchId));

        var inventory = await _inventories.Find(filter).FirstOrDefaultAsync();

        if (inventory == null)
        {
            inventory = new Inventory
            {
                ProductId = adjustment.ProductId,
                BranchId = adjustment.BranchId,
                Quantity = adjustment.Quantity,
                ReservedQuantity = 0
            };
            await _inventories.InsertOneAsync(inventory);
        }
        else
        {
            inventory.Quantity += adjustment.Quantity;
            inventory.UpdatedAt = DateTime.UtcNow;
            await _inventories.ReplaceOneAsync(filter, inventory);
        }

        return ServiceResult<int>.Ok(inventory.Quantity);
    }

    public async Task<List<InventoryDTO>> GetLowStockAsync(string? branchId = null)
    {
        var filter = Builders<Inventory>.Filter.Empty;

        if (!string.IsNullOrEmpty(branchId))
        {
            filter &= Builders<Inventory>.Filter.Eq(i => i.BranchId, branchId);
        }

        // We need to find products with min stock level and compare
        // This requires a more complex query in MongoDB
        var productsWithMinStock = await _products.Find(p => p.MinStockLevel.HasValue).ToListAsync();
        var productIdsWithMinStock = productsWithMinStock.Select(p => p.Id).ToList();

        filter &= Builders<Inventory>.Filter.In(i => i.ProductId, productIdsWithMinStock);

        var inventories = await _inventories.Find(filter).ToListAsync();

        // Filter by comparing quantity with min stock level
        var lowStockInventories = new List<Inventory>();
        var productDict = productsWithMinStock.ToDictionary(p => p.Id, p => p.MinStockLevel!.Value);

        foreach (var inv in inventories)
        {
            if (productDict.TryGetValue(inv.ProductId, out var minStock) && inv.Quantity <= minStock)
            {
                lowStockInventories.Add(inv);
            }
        }

        var productIds = lowStockInventories.Select(i => i.ProductId).Distinct().ToList();
        var branchIds = lowStockInventories.Select(i => i.BranchId).Distinct().ToList();

        var products = await _products.Find(p => productIds.Contains(p.Id)).ToListAsync();
        var branches = await _branches.Find(b => branchIds.Contains(b.Id)).ToListAsync();

        var productNameDict = products.ToDictionary(p => p.Id, p => p.Name);
        var branchNameDict = branches.ToDictionary(b => b.Id, b => b.Name);

        return lowStockInventories.Select(i => ToDto(i, productNameDict, branchNameDict)).ToList();
    }

    private static InventoryDTO ToDto(Inventory i, Dictionary<string, string> productDict, Dictionary<string, string> branchDict)
    {
        productDict.TryGetValue(i.ProductId, out var productName);
        branchDict.TryGetValue(i.BranchId, out var branchName);

        return new InventoryDTO
        {
            Id = i.Id,
            ProductId = i.ProductId,
            BranchId = i.BranchId,
            Quantity = i.Quantity,
            ReservedQuantity = i.ReservedQuantity,
            ProductName = productName,
            BranchName = branchName
        };
    }
}