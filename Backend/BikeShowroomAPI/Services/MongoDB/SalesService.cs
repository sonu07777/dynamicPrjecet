using MongoDB.Driver;
using MongoDB.Bson;
using BikeShowroomAPI.Data.MongoDB;
using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Models.MongoDB;

namespace BikeShowroomAPI.Services.MongoDB;

public class SalesService : ISalesService
{
    private readonly IMongoCollection<Sale> _sales;
    private readonly IMongoCollection<Product> _products;
    private readonly IMongoCollection<Customer> _customers;
    private readonly IMongoCollection<Inventory> _inventories;
    private readonly IMongoCollection<Branch> _branches;
    private readonly IMongoCollection<Company> _companies;

    public SalesService(MongoDbContext context)
    {
        _sales = context.Sales;
        _products = context.Products;
        _customers = context.Customers;
        _inventories = context.Inventories;
        _branches = context.Branches;
        _companies = context.Companies;
    }

    public async Task<List<SaleDTO>> GetSalesAsync(string? companyId = null, string? branchId = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var filter = Builders<Sale>.Filter.Empty;

        if (!string.IsNullOrEmpty(companyId))
        {
            filter &= Builders<Sale>.Filter.Eq(s => s.CompanyId, companyId);
        }

        if (!string.IsNullOrEmpty(branchId))
        {
            filter &= Builders<Sale>.Filter.Eq(s => s.BranchId, branchId);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filter &= Builders<Sale>.Filter.Eq(s => s.Status, status);
        }

        if (fromDate.HasValue)
        {
            filter &= Builders<Sale>.Filter.Gte(s => s.SaleDate, fromDate.Value);
        }

        if (toDate.HasValue)
        {
            filter &= Builders<Sale>.Filter.Lte(s => s.SaleDate, toDate.Value);
        }

        var sales = await _sales.Find(filter)
            .SortByDescending(s => s.SaleDate)
            .ToListAsync();

        // Get related data
        var customerIds = sales
            .Where(s => !string.IsNullOrWhiteSpace(s.CustomerId))
            .Select(s => s.CustomerId!)
            .Distinct()
            .ToList();
        var branchIds = sales.Select(s => s.BranchId).Distinct().ToList();
        var allItemProductIds = sales.SelectMany(s => s.Items.Select(i => i.ProductId)).Distinct().ToList();

        var customers = customerIds.Count == 0
            ? new List<Customer>()
            : await _customers.Find(c => customerIds.Contains(c.Id)).ToListAsync();
        var branches = await _branches.Find(b => branchIds.Contains(b.Id)).ToListAsync();
        var products = await _products.Find(p => allItemProductIds.Contains(p.Id)).ToListAsync();

        var customerDict = customers.ToDictionary(c => c.Id, c => $"{c.FirstName} {c.LastName}".Trim());
        var branchDict = branches.ToDictionary(b => b.Id, b => b.Name);
        var productDict = products.ToDictionary(p => p.Id, p => p.Name);

        return sales.Select(s => ToDto(s, customerDict, branchDict, productDict)).ToList();
    }

    public async Task<SaleDTO?> GetSaleAsync(string id)
    {
        var sale = await _sales.Find(s => s.Id == id).FirstOrDefaultAsync();

        if (sale == null) return null;

        var customer = string.IsNullOrWhiteSpace(sale.CustomerId)
            ? null
            : await _customers.Find(c => c.Id == sale.CustomerId).FirstOrDefaultAsync();
        var branch = await _branches.Find(b => b.Id == sale.BranchId).FirstOrDefaultAsync();

        var productIds = sale.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _products.Find(p => productIds.Contains(p.Id)).ToListAsync();
        var productDict = products.ToDictionary(p => p.Id, p => p.Name);

        var customerName = customer != null ? $"{customer.FirstName} {customer.LastName}".Trim() : "";

        var customerNames = customer == null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string> { [customer.Id] = customerName };

        return ToDto(sale,
            customerNames,
            new Dictionary<string, string> { [sale.BranchId] = branch?.Name ?? "" },
            productDict);
    }

    public async Task<ServiceResult<SaleDTO>> CreateSaleAsync(CreateSaleDTO createDto, string createdBy)
    {
        // Validate company, branch, customer exist
        var companyExists = await _companies.Find(c => c.Id == createDto.CompanyId && c.IsActive).AnyAsync();
        if (!companyExists)
            return ServiceResult<SaleDTO>.Fail("Company not found", ServiceResultStatus.NotFound);

        var branchExists = await _branches.Find(b => b.Id == createDto.BranchId && b.IsActive).AnyAsync();
        if (!branchExists)
            return ServiceResult<SaleDTO>.Fail("Branch not found", ServiceResultStatus.NotFound);

        var customerId = string.IsNullOrWhiteSpace(createDto.CustomerId) ? null : createDto.CustomerId.Trim();
        if (customerId != null)
        {
            if (!ObjectId.TryParse(customerId, out _))
                return ServiceResult<SaleDTO>.Fail("Customer not found", ServiceResultStatus.NotFound);

            var customerExists = await _customers.Find(c =>
                c.Id == customerId && c.CompanyId == createDto.CompanyId && c.IsActive).AnyAsync();
            if (!customerExists)
                return ServiceResult<SaleDTO>.Fail("Customer not found", ServiceResultStatus.NotFound);
        }

        var saleNumber = $"SL-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var sale = new Sale
        {
            CompanyId = createDto.CompanyId,
            BranchId = createDto.BranchId,
            CustomerId = customerId,
            SaleNumber = saleNumber,
            SaleDate = DateTime.UtcNow,
            Status = "Completed",
            PaymentMethod = createDto.PaymentMethod,
            Notes = createDto.Notes,
            CreatedBy = createdBy,
            TotalAmount = createDto.Items.Sum(i => i.Quantity * i.UnitPrice)
        };

        // Check inventory availability before creating sale
        foreach (var itemDto in createDto.Items)
        {
            var filter = Builders<Inventory>.Filter.And(
                Builders<Inventory>.Filter.Eq(i => i.ProductId, itemDto.ProductId),
                Builders<Inventory>.Filter.Eq(i => i.BranchId, createDto.BranchId));

            var inventory = await _inventories.Find(filter).FirstOrDefaultAsync();

            if (inventory == null || inventory.Quantity < itemDto.Quantity)
            {
                var product = await _products.Find(p => p.Id == itemDto.ProductId).FirstOrDefaultAsync();
                return ServiceResult<SaleDTO>.Fail(
                    $"Insufficient stock for product: {product?.Name ?? itemDto.ProductId}. Available: {inventory?.Quantity ?? 0}, Required: {itemDto.Quantity}",
                    ServiceResultStatus.BadRequest);
            }
        }

        // Create sale items and update inventory
        foreach (var itemDto in createDto.Items)
        {
            sale.Items.Add(new SaleItem
            {
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = itemDto.UnitPrice,
                TotalPrice = itemDto.Quantity * itemDto.UnitPrice
            });

            // Update inventory
            var filter = Builders<Inventory>.Filter.And(
                Builders<Inventory>.Filter.Eq(i => i.ProductId, itemDto.ProductId),
                Builders<Inventory>.Filter.Eq(i => i.BranchId, createDto.BranchId));

            var inventory = await _inventories.Find(filter).FirstOrDefaultAsync();
            if (inventory != null)
            {
                inventory.Quantity -= itemDto.Quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                await _inventories.ReplaceOneAsync(filter, inventory);
            }
        }

        await _sales.InsertOneAsync(sale);

        var customer = customerId == null
            ? null
            : await _customers.Find(c => c.Id == customerId).FirstOrDefaultAsync();
        var branch = await _branches.Find(b => b.Id == createDto.BranchId).FirstOrDefaultAsync();
        var productIds = sale.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _products.Find(p => productIds.Contains(p.Id)).ToListAsync();
        var productDict = products.ToDictionary(p => p.Id, p => p.Name);

        var customerName = customer != null ? $"{customer.FirstName} {customer.LastName}".Trim() : "";

        var customerNames = customer == null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string> { [customer.Id] = customerName };

        return ServiceResult<SaleDTO>.Ok(ToDto(sale,
            customerNames,
            new Dictionary<string, string> { [sale.BranchId] = branch?.Name ?? "" },
            productDict));
    }

    public async Task<ServiceResult> UpdateSaleStatusAsync(string id, string status)
    {
        var sale = await _sales.Find(s => s.Id == id).FirstOrDefaultAsync();

        if (sale == null)
            return ServiceResult.Fail("Sale not found", ServiceResultStatus.NotFound);

        sale.Status = status;
        await _sales.ReplaceOneAsync(s => s.Id == id, sale);

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteSaleAsync(string id)
    {
        var sale = await _sales.Find(s => s.Id == id).FirstOrDefaultAsync();
        if (sale == null)
            return ServiceResult.Fail("Sale not found", ServiceResultStatus.NotFound);

        // Restore inventory when deleting a sale
        foreach (var item in sale.Items)
        {
            var filter = Builders<Inventory>.Filter.And(
                Builders<Inventory>.Filter.Eq(i => i.ProductId, item.ProductId),
                Builders<Inventory>.Filter.Eq(i => i.BranchId, sale.BranchId));

            var inventory = await _inventories.Find(filter).FirstOrDefaultAsync();
            if (inventory != null)
            {
                inventory.Quantity += item.Quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                await _inventories.ReplaceOneAsync(filter, inventory);
            }
        }

        await _sales.DeleteOneAsync(s => s.Id == id);
        return ServiceResult.Ok();
    }

    private static SaleDTO ToDto(Sale sale,
        Dictionary<string, string> customerDict,
        Dictionary<string, string> branchDict,
        Dictionary<string, string> productDict)
    {
        string? customerName;
        if (sale.CustomerId is not null)
            customerDict.TryGetValue(sale.CustomerId, out customerName);
        else
            customerName = null;
        branchDict.TryGetValue(sale.BranchId, out var branchName);

        return new SaleDTO
        {
            Id = sale.Id,
            CompanyId = sale.CompanyId,
            BranchId = sale.BranchId,
            CustomerId = sale.CustomerId,
            SaleNumber = sale.SaleNumber,
            SaleDate = sale.SaleDate,
            Status = sale.Status,
            PaymentMethod = sale.PaymentMethod,
            TotalAmount = sale.TotalAmount,
            Notes = sale.Notes,
            CreatedBy = sale.CreatedBy,
            CustomerName = customerName,
            BranchName = branchName,
            Items = sale.Items.Select(i =>
            {
                productDict.TryGetValue(i.ProductId, out var productName);
                return new SaleItemDTO
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = productName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                };
            }).ToList()
        };
    }
}