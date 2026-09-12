using MongoDB.Driver;
using BikeShowroomAPI.Data.MongoDB;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models.MongoDB;

namespace BikeShowroomAPI.Services.MongoDB;

public class StockTransferService : IStockTransferService
{
    private readonly IMongoCollection<StockTransfer> _stockTransfers;
    private readonly IMongoCollection<Product> _products;
    private readonly IMongoCollection<Branch> _branches;
    private readonly IMongoCollection<Inventory> _inventories;
    private readonly IMongoCollection<Company> _companies;

    public StockTransferService(MongoDbContext context)
    {
        _stockTransfers = context.StockTransfers;
        _products = context.Products;
        _branches = context.Branches;
        _inventories = context.Inventories;
        _companies = context.Companies;
    }

    public async Task<List<StockTransferDTO>> GetStockTransfersAsync(string? companyId = null, string? fromBranchId = null, string? toBranchId = null, string? status = null)
    {
        var filter = Builders<StockTransfer>.Filter.Empty;

        if (!string.IsNullOrEmpty(companyId))
        {
            filter &= Builders<StockTransfer>.Filter.Eq(st => st.CompanyId, companyId);
        }

        if (!string.IsNullOrEmpty(fromBranchId))
        {
            filter &= Builders<StockTransfer>.Filter.Eq(st => st.FromBranchId, fromBranchId);
        }

        if (!string.IsNullOrEmpty(toBranchId))
        {
            filter &= Builders<StockTransfer>.Filter.Eq(st => st.ToBranchId, toBranchId);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filter &= Builders<StockTransfer>.Filter.Eq(st => st.Status, status);
        }

        var transfers = await _stockTransfers.Find(filter)
            .SortByDescending(st => st.TransferDate)
            .ToListAsync();

        // Get related data
        var fromBranchIds = transfers.Select(st => st.FromBranchId).Distinct().ToList();
        var toBranchIds = transfers.Select(st => st.ToBranchId).Distinct().ToList();
        var allItemProductIds = transfers.SelectMany(st => st.Items.Select(i => i.ProductId)).Distinct().ToList();

        var fromBranches = await _branches.Find(b => fromBranchIds.Contains(b.Id)).ToListAsync();
        var toBranches = await _branches.Find(b => toBranchIds.Contains(b.Id)).ToListAsync();
        var products = await _products.Find(p => allItemProductIds.Contains(p.Id)).ToListAsync();

        var fromBranchDict = fromBranches.ToDictionary(b => b.Id, b => b.Name);
        var toBranchDict = toBranches.ToDictionary(b => b.Id, b => b.Name);
        var productDict = products.ToDictionary(p => p.Id, p => p.Name);

        return transfers.Select(st => ToDto(st, fromBranchDict, toBranchDict, productDict)).ToList();
    }

    public async Task<StockTransferDTO?> GetStockTransferAsync(string id)
    {
        var transfer = await _stockTransfers.Find(st => st.Id == id).FirstOrDefaultAsync();

        if (transfer == null) return null;

        var fromBranch = await _branches.Find(b => b.Id == transfer.FromBranchId).FirstOrDefaultAsync();
        var toBranch = await _branches.Find(b => b.Id == transfer.ToBranchId).FirstOrDefaultAsync();

        var productIds = transfer.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _products.Find(p => productIds.Contains(p.Id)).ToListAsync();
        var productDict = products.ToDictionary(p => p.Id, p => p.Name);

        return ToDto(transfer,
            new Dictionary<string, string> { [transfer.FromBranchId] = fromBranch?.Name ?? "" },
            new Dictionary<string, string> { [transfer.ToBranchId] = toBranch?.Name ?? "" },
            productDict);
    }

    public async Task<ServiceResult<StockTransferDTO>> CreateStockTransferAsync(CreateStockTransferDTO createDto, string createdBy)
    {
        // Validate company, branches exist
        var companyExists = await _companies.Find(c => c.Id == createDto.CompanyId && c.IsActive).AnyAsync();
        if (!companyExists)
            return ServiceResult<StockTransferDTO>.Fail("Company not found", ServiceResultStatus.NotFound);

        var fromBranchExists = await _branches.Find(b => b.Id == createDto.FromBranchId && b.IsActive).AnyAsync();
        if (!fromBranchExists)
            return ServiceResult<StockTransferDTO>.Fail("Source branch not found", ServiceResultStatus.NotFound);

        var toBranchExists = await _branches.Find(b => b.Id == createDto.ToBranchId && b.IsActive).AnyAsync();
        if (!toBranchExists)
            return ServiceResult<StockTransferDTO>.Fail("Destination branch not found", ServiceResultStatus.NotFound);

        if (createDto.FromBranchId == createDto.ToBranchId)
            return ServiceResult<StockTransferDTO>.Fail("Source and destination branches cannot be the same", ServiceResultStatus.BadRequest);

        var transferNumber = $"ST-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var transfer = new StockTransfer
        {
            CompanyId = createDto.CompanyId,
            FromBranchId = createDto.FromBranchId,
            ToBranchId = createDto.ToBranchId,
            TransferNumber = transferNumber,
            TransferDate = DateTime.UtcNow,
            Status = "Pending",
            Notes = createDto.Notes,
            InitiatedBy = createdBy
        };

        foreach (var itemDto in createDto.Items)
        {
            transfer.Items.Add(new StockTransferItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity
            });
        }

        await _stockTransfers.InsertOneAsync(transfer);

        var fromBranch = await _branches.Find(b => b.Id == createDto.FromBranchId).FirstOrDefaultAsync();
        var toBranch = await _branches.Find(b => b.Id == createDto.ToBranchId).FirstOrDefaultAsync();
        var productIds = transfer.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _products.Find(p => productIds.Contains(p.Id)).ToListAsync();
        var productDict = products.ToDictionary(p => p.Id, p => p.Name);

        return ServiceResult<StockTransferDTO>.Ok(ToDto(transfer,
            new Dictionary<string, string> { [transfer.FromBranchId] = fromBranch?.Name ?? "" },
            new Dictionary<string, string> { [transfer.ToBranchId] = toBranch?.Name ?? "" },
            productDict));
    }

    public async Task<ServiceResult> UpdateStockTransferStatusAsync(string id, string status)
    {
        var transfer = await _stockTransfers.Find(st => st.Id == id).FirstOrDefaultAsync();

        if (transfer == null)
            return ServiceResult.Fail("Stock transfer not found", ServiceResultStatus.NotFound);

        var currentStatus = transfer.Status;
        transfer.Status = status;

        if (status == "InTransit")
        {
            // Deduct from source branch
            foreach (var item in transfer.Items)
            {
                var filter = Builders<Inventory>.Filter.And(
                    Builders<Inventory>.Filter.Eq(i => i.ProductId, item.ProductId),
                    Builders<Inventory>.Filter.Eq(i => i.BranchId, transfer.FromBranchId));

                var inventory = await _inventories.Find(filter).FirstOrDefaultAsync();

                if (inventory != null)
                {
                    if (inventory.Quantity < item.Quantity)
                    {
                        return ServiceResult.Fail(
                            $"Insufficient stock in source branch for product {item.ProductId}. Available: {inventory.Quantity}, Required: {item.Quantity}",
                            ServiceResultStatus.BadRequest);
                    }

                    inventory.Quantity -= item.Quantity;
                    inventory.UpdatedAt = DateTime.UtcNow;
                    await _inventories.ReplaceOneAsync(filter, inventory);
                }
                else
                {
                    return ServiceResult.Fail(
                        $"Product {item.ProductId} not found in source branch inventory",
                        ServiceResultStatus.BadRequest);
                }
            }
        }

        if (status == "Received")
        {
            // Add to destination branch
            foreach (var item in transfer.Items)
            {
                var filter = Builders<Inventory>.Filter.And(
                    Builders<Inventory>.Filter.Eq(i => i.ProductId, item.ProductId),
                    Builders<Inventory>.Filter.Eq(i => i.BranchId, transfer.ToBranchId));

                var inventory = await _inventories.Find(filter).FirstOrDefaultAsync();

                if (inventory != null)
                {
                    inventory.Quantity += item.Quantity;
                    inventory.UpdatedAt = DateTime.UtcNow;
                    await _inventories.ReplaceOneAsync(filter, inventory);
                }
                else
                {
                    // Create inventory if it doesn't exist
                    var newInventory = new Inventory
                    {
                        ProductId = item.ProductId,
                        BranchId = transfer.ToBranchId,
                        Quantity = item.Quantity,
                        ReservedQuantity = 0
                    };
                    await _inventories.InsertOneAsync(newInventory);
                }
            }
        }

        if (status == "Cancelled" && currentStatus == "InTransit")
        {
            // Restore source branch inventory
            foreach (var item in transfer.Items)
            {
                var filter = Builders<Inventory>.Filter.And(
                    Builders<Inventory>.Filter.Eq(i => i.ProductId, item.ProductId),
                    Builders<Inventory>.Filter.Eq(i => i.BranchId, transfer.FromBranchId));

                var inventory = await _inventories.Find(filter).FirstOrDefaultAsync();

                if (inventory != null)
                {
                    inventory.Quantity += item.Quantity;
                    inventory.UpdatedAt = DateTime.UtcNow;
                    await _inventories.ReplaceOneAsync(filter, inventory);
                }
            }
        }

        await _stockTransfers.ReplaceOneAsync(st => st.Id == id, transfer);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteStockTransferAsync(string id)
    {
        var transfer = await _stockTransfers.Find(st => st.Id == id).FirstOrDefaultAsync();
        if (transfer == null)
            return ServiceResult.Fail("Stock transfer not found", ServiceResultStatus.NotFound);

        // If transfer was InTransit, restore source branch inventory
        if (transfer.Status == "InTransit")
        {
            foreach (var item in transfer.Items)
            {
                var filter = Builders<Inventory>.Filter.And(
                    Builders<Inventory>.Filter.Eq(i => i.ProductId, item.ProductId),
                    Builders<Inventory>.Filter.Eq(i => i.BranchId, transfer.FromBranchId));

                var inventory = await _inventories.Find(filter).FirstOrDefaultAsync();
                if (inventory != null)
                {
                    inventory.Quantity += item.Quantity;
                    inventory.UpdatedAt = DateTime.UtcNow;
                    await _inventories.ReplaceOneAsync(filter, inventory);
                }
            }
        }

        await _stockTransfers.DeleteOneAsync(st => st.Id == id);
        return ServiceResult.Ok();
    }

    private static StockTransferDTO ToDto(StockTransfer transfer,
        Dictionary<string, string> fromBranchDict,
        Dictionary<string, string> toBranchDict,
        Dictionary<string, string> productDict)
    {
        fromBranchDict.TryGetValue(transfer.FromBranchId, out var fromBranchName);
        toBranchDict.TryGetValue(transfer.ToBranchId, out var toBranchName);

        return new StockTransferDTO
        {
            Id = transfer.Id,
            CompanyId = transfer.CompanyId,
            FromBranchId = transfer.FromBranchId,
            ToBranchId = transfer.ToBranchId,
            TransferNumber = transfer.TransferNumber,
            TransferDate = transfer.TransferDate,
            Status = transfer.Status,
            Notes = transfer.Notes,
            InitiatedBy = transfer.InitiatedBy,
            FromBranchName = fromBranchName,
            ToBranchName = toBranchName,
            Items = transfer.Items.Select(i =>
            {
                productDict.TryGetValue(i.ProductId, out var productName);
                return new StockTransferItemDTO
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = productName,
                    Quantity = i.Quantity
                };
            }).ToList()
        };
    }
}