using MongoDB.Driver;
using BikeShowroomAPI.Data.MongoDB;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models.MongoDB;

namespace BikeShowroomAPI.Services.MongoDB;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IMongoCollection<PurchaseOrder> _purchaseOrders;
    private readonly IMongoCollection<Supplier> _suppliers;
    private readonly IMongoCollection<Branch> _branches;
    private readonly IMongoCollection<Product> _products;
    private readonly IMongoCollection<Inventory> _inventories;
    private readonly IMongoCollection<Company> _companies;

    public PurchaseOrderService(MongoDbContext context)
    {
        _purchaseOrders = context.PurchaseOrders;
        _suppliers = context.Suppliers;
        _branches = context.Branches;
        _products = context.Products;
        _inventories = context.Inventories;
        _companies = context.Companies;
    }

    public async Task<List<PurchaseOrderDTO>> GetPurchaseOrdersAsync(string? companyId = null, string? branchId = null, string? status = null)
    {
        var filter = Builders<PurchaseOrder>.Filter.Empty;

        if (!string.IsNullOrEmpty(companyId))
        {
            filter &= Builders<PurchaseOrder>.Filter.Eq(po => po.CompanyId, companyId);
        }

        if (!string.IsNullOrEmpty(branchId))
        {
            filter &= Builders<PurchaseOrder>.Filter.Eq(po => po.BranchId, branchId);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filter &= Builders<PurchaseOrder>.Filter.Eq(po => po.Status, status);
        }

        var orders = await _purchaseOrders.Find(filter)
            .SortByDescending(po => po.OrderDate)
            .ToListAsync();

        // Get related data
        var supplierIds = orders.Select(po => po.SupplierId).Distinct().ToList();
        var branchIds = orders.Select(po => po.BranchId).Distinct().ToList();

        var suppliers = await _suppliers.Find(s => supplierIds.Contains(s.Id)).ToListAsync();
        var branches = await _branches.Find(b => branchIds.Contains(b.Id)).ToListAsync();

        var supplierDict = suppliers.ToDictionary(s => s.Id, s => s.Name);
        var branchDict = branches.ToDictionary(b => b.Id, b => b.Name);

        // Get product names for items
        var allItemProductIds = orders.SelectMany(po => po.Items.Select(i => i.ProductId)).Distinct().ToList();
        var products = await _products.Find(p => allItemProductIds.Contains(p.Id)).ToListAsync();
        var productDict = products.ToDictionary(p => p.Id, p => p.Name);

        return orders.Select(po => ToDto(po, supplierDict, branchDict, productDict)).ToList();
    }

    public async Task<PurchaseOrderDTO?> GetPurchaseOrderAsync(string id)
    {
        var po = await _purchaseOrders.Find(p => p.Id == id).FirstOrDefaultAsync();

        if (po == null) return null;

        var supplier = await _suppliers.Find(s => s.Id == po.SupplierId).FirstOrDefaultAsync();
        var branch = await _branches.Find(b => b.Id == po.BranchId).FirstOrDefaultAsync();

        var productIds = po.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _products.Find(p => productIds.Contains(p.Id)).ToListAsync();
        var productDict = products.ToDictionary(p => p.Id, p => p.Name);

        return ToDto(po,
            new Dictionary<string, string> { [po.SupplierId] = supplier?.Name ?? "" },
            new Dictionary<string, string> { [po.BranchId] = branch?.Name ?? "" },
            productDict);
    }

    public async Task<ServiceResult<PurchaseOrderDTO>> CreatePurchaseOrderAsync(CreatePurchaseOrderDTO createDto, string createdBy)
    {
        // Validate company, branch, supplier exist
        var companyExists = await _companies.Find(c => c.Id == createDto.CompanyId && c.IsActive).AnyAsync();
        if (!companyExists)
            return ServiceResult<PurchaseOrderDTO>.Fail("Company not found", ServiceResultStatus.NotFound);

        var branchExists = await _branches.Find(b => b.Id == createDto.BranchId && b.IsActive).AnyAsync();
        if (!branchExists)
            return ServiceResult<PurchaseOrderDTO>.Fail("Branch not found", ServiceResultStatus.NotFound);

        var supplierExists = await _suppliers.Find(s => s.Id == createDto.SupplierId && s.IsActive).AnyAsync();
        if (!supplierExists)
            return ServiceResult<PurchaseOrderDTO>.Fail("Supplier not found", ServiceResultStatus.NotFound);

        var orderNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var purchaseOrder = new PurchaseOrder
        {
            CompanyId = createDto.CompanyId,
            BranchId = createDto.BranchId,
            SupplierId = createDto.SupplierId,
            OrderNumber = orderNumber,
            OrderDate = DateTime.UtcNow,
            ExpectedDeliveryDate = createDto.ExpectedDeliveryDate,
            Status = "Pending",
            Notes = createDto.Notes,
            CreatedBy = createdBy,
            TotalAmount = createDto.Items.Sum(i => i.QuantityOrdered * i.UnitPrice)
        };

        foreach (var itemDto in createDto.Items)
        {
            purchaseOrder.Items.Add(new PurchaseOrderItem
            {
                ProductId = itemDto.ProductId,
                QuantityOrdered = itemDto.QuantityOrdered,
                UnitPrice = itemDto.UnitPrice,
                TotalPrice = itemDto.QuantityOrdered * itemDto.UnitPrice
            });
        }

        await _purchaseOrders.InsertOneAsync(purchaseOrder);

        var supplier = await _suppliers.Find(s => s.Id == purchaseOrder.SupplierId).FirstOrDefaultAsync();
        var branch = await _branches.Find(b => b.Id == purchaseOrder.BranchId).FirstOrDefaultAsync();

        return ServiceResult<PurchaseOrderDTO>.Ok(ToDto(purchaseOrder,
            new Dictionary<string, string> { [purchaseOrder.SupplierId] = supplier?.Name ?? "" },
            new Dictionary<string, string> { [purchaseOrder.BranchId] = branch?.Name ?? "" },
            new Dictionary<string, string>()));
    }

    public async Task<ServiceResult> UpdatePurchaseOrderStatusAsync(string id, string status, Dictionary<string, int>? receivedQuantities = null)
    {
        var po = await _purchaseOrders.Find(p => p.Id == id).FirstOrDefaultAsync();

        if (po == null)
            return ServiceResult.Fail("Purchase order not found", ServiceResultStatus.NotFound);

        po.Status = status;

        if (status == "Received" || status == "PartiallyReceived")
        {
            po.ReceivedDate = DateTime.UtcNow;

            foreach (var item in po.Items)
            {
                var receivedQty = status == "Received"
                    ? item.QuantityOrdered
                    : item.QuantityReceived + (receivedQuantities?.GetValueOrDefault(item.ProductId, 0) ?? 0);

                var previousReceived = item.QuantityReceived;
                item.QuantityReceived = Math.Min(receivedQty, item.QuantityOrdered);

                // Calculate quantity to add to inventory
                var qtyToAdd = item.QuantityReceived - previousReceived;

                // Update inventory
                var filter = Builders<Inventory>.Filter.And(
                    Builders<Inventory>.Filter.Eq(i => i.ProductId, item.ProductId),
                    Builders<Inventory>.Filter.Eq(i => i.BranchId, po.BranchId));

                var inventory = await _inventories.Find(filter).FirstOrDefaultAsync();

                if (inventory != null)
                {
                    inventory.Quantity += qtyToAdd;
                    inventory.UpdatedAt = DateTime.UtcNow;
                    await _inventories.ReplaceOneAsync(filter, inventory);
                }
                else
                {
                    // Create inventory if it doesn't exist
                    var newInventory = new Inventory
                    {
                        ProductId = item.ProductId,
                        BranchId = po.BranchId,
                        Quantity = qtyToAdd,
                        ReservedQuantity = 0
                    };
                    await _inventories.InsertOneAsync(newInventory);
                }
            }

            if (po.Items.All(i => i.QuantityReceived >= i.QuantityOrdered))
                po.Status = "Received";
            else if (po.Items.Any(i => i.QuantityReceived > 0))
                po.Status = "PartiallyReceived";
        }

        if (status == "Cancelled")
        {
            po.Status = "Cancelled";
        }

        await _purchaseOrders.ReplaceOneAsync(p => p.Id == id, po);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeletePurchaseOrderAsync(string id)
    {
        var po = await _purchaseOrders.Find(p => p.Id == id).FirstOrDefaultAsync();
        if (po == null)
            return ServiceResult.Fail("Purchase order not found", ServiceResultStatus.NotFound);

        await _purchaseOrders.DeleteOneAsync(p => p.Id == id);
        return ServiceResult.Ok();
    }

    private static PurchaseOrderDTO ToDto(PurchaseOrder po,
        Dictionary<string, string> supplierDict,
        Dictionary<string, string> branchDict,
        Dictionary<string, string> productDict)
    {
        supplierDict.TryGetValue(po.SupplierId, out var supplierName);
        branchDict.TryGetValue(po.BranchId, out var branchName);

        return new PurchaseOrderDTO
        {
            Id = po.Id,
            CompanyId = po.CompanyId,
            BranchId = po.BranchId,
            SupplierId = po.SupplierId,
            OrderNumber = po.OrderNumber,
            OrderDate = po.OrderDate,
            ExpectedDeliveryDate = po.ExpectedDeliveryDate,
            ReceivedDate = po.ReceivedDate,
            Status = po.Status,
            TotalAmount = po.TotalAmount,
            Notes = po.Notes,
            CreatedBy = po.CreatedBy,
            SupplierName = supplierName,
            BranchName = branchName,
            Items = po.Items.Select(i =>
            {
                productDict.TryGetValue(i.ProductId, out var productName);
                return new PurchaseOrderItemDTO
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = productName,
                    QuantityOrdered = i.QuantityOrdered,
                    QuantityReceived = i.QuantityReceived,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                };
            }).ToList()
        };
    }
}