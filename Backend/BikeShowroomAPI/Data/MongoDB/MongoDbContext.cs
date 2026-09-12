using MongoDB.Driver;
using BikeShowroomAPI.Models.MongoDB;
using Microsoft.Extensions.Options;

namespace BikeShowroomAPI.Data.MongoDB;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<Company> Companies => _database.GetCollection<Company>("companies");
    public IMongoCollection<Branch> Branches => _database.GetCollection<Branch>("branches");
    public IMongoCollection<Category> Categories => _database.GetCollection<Category>("categories");
    public IMongoCollection<Product> Products => _database.GetCollection<Product>("products");
    public IMongoCollection<Customer> Customers => _database.GetCollection<Customer>("customers");
    public IMongoCollection<Supplier> Suppliers => _database.GetCollection<Supplier>("suppliers");
    public IMongoCollection<Inventory> Inventories => _database.GetCollection<Inventory>("inventories");
    public IMongoCollection<Sale> Sales => _database.GetCollection<Sale>("sales");
    public IMongoCollection<PurchaseOrder> PurchaseOrders => _database.GetCollection<PurchaseOrder>("purchaseOrders");
    public IMongoCollection<StockTransfer> StockTransfers => _database.GetCollection<StockTransfer>("stockTransfers");
    public IMongoCollection<ApplicationUser> Users => _database.GetCollection<ApplicationUser>("users");

    public async Task EnsureIndexesAsync()
    {
        // Companies
        await Companies.Indexes.CreateOneAsync(new CreateIndexModel<Company>(
            Builders<Company>.IndexKeys.Ascending(c => c.Name)));

        // Branches
        await Branches.Indexes.CreateOneAsync(new CreateIndexModel<Branch>(
            Builders<Branch>.IndexKeys.Ascending(b => b.CompanyId)));
        await Branches.Indexes.CreateOneAsync(new CreateIndexModel<Branch>(
            Builders<Branch>.IndexKeys.Ascending(b => b.Code), new CreateIndexOptions { Unique = true }));

        // Categories
        await Categories.Indexes.CreateOneAsync(new CreateIndexModel<Category>(
            Builders<Category>.IndexKeys.Ascending(c => c.CompanyId)));
        await Categories.Indexes.CreateOneAsync(new CreateIndexModel<Category>(
            Builders<Category>.IndexKeys.Ascending(c => c.ParentCategoryId)));

        // Products
        await Products.Indexes.CreateOneAsync(new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.CompanyId)));
        await Products.Indexes.CreateOneAsync(new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.CategoryId)));
        await Products.Indexes.CreateOneAsync(new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.SKU), new CreateIndexOptions { Unique = true }));
        await Products.Indexes.CreateOneAsync(new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.Barcode)));

        // Customers
        await Customers.Indexes.CreateOneAsync(new CreateIndexModel<Customer>(
            Builders<Customer>.IndexKeys.Ascending(c => c.CompanyId)));
        await Customers.Indexes.CreateOneAsync(new CreateIndexModel<Customer>(
            Builders<Customer>.IndexKeys.Ascending(c => c.Email)));

        // Suppliers
        await Suppliers.Indexes.CreateOneAsync(new CreateIndexModel<Supplier>(
            Builders<Supplier>.IndexKeys.Ascending(s => s.CompanyId)));

        // Inventories
        await Inventories.Indexes.CreateOneAsync(new CreateIndexModel<Inventory>(
            Builders<Inventory>.IndexKeys.Ascending(i => i.ProductId).Ascending(i => i.BranchId),
            new CreateIndexOptions { Unique = true }));
        await Inventories.Indexes.CreateOneAsync(new CreateIndexModel<Inventory>(
            Builders<Inventory>.IndexKeys.Ascending(i => i.BranchId)));

        // Sales
        await Sales.Indexes.CreateOneAsync(new CreateIndexModel<Sale>(
            Builders<Sale>.IndexKeys.Ascending(s => s.CompanyId)));
        await Sales.Indexes.CreateOneAsync(new CreateIndexModel<Sale>(
            Builders<Sale>.IndexKeys.Ascending(s => s.BranchId)));
        await Sales.Indexes.CreateOneAsync(new CreateIndexModel<Sale>(
            Builders<Sale>.IndexKeys.Ascending(s => s.CustomerId)));
        await Sales.Indexes.CreateOneAsync(new CreateIndexModel<Sale>(
            Builders<Sale>.IndexKeys.Ascending(s => s.InvoiceNumber), new CreateIndexOptions { Unique = true }));
        await Sales.Indexes.CreateOneAsync(new CreateIndexModel<Sale>(
            Builders<Sale>.IndexKeys.Descending(s => s.SaleDate)));

        // PurchaseOrders
        await PurchaseOrders.Indexes.CreateOneAsync(new CreateIndexModel<PurchaseOrder>(
            Builders<PurchaseOrder>.IndexKeys.Ascending(po => po.CompanyId)));
        await PurchaseOrders.Indexes.CreateOneAsync(new CreateIndexModel<PurchaseOrder>(
            Builders<PurchaseOrder>.IndexKeys.Ascending(po => po.BranchId)));
        await PurchaseOrders.Indexes.CreateOneAsync(new CreateIndexModel<PurchaseOrder>(
            Builders<PurchaseOrder>.IndexKeys.Ascending(po => po.SupplierId)));
        await PurchaseOrders.Indexes.CreateOneAsync(new CreateIndexModel<PurchaseOrder>(
            Builders<PurchaseOrder>.IndexKeys.Ascending(po => po.OrderNumber), new CreateIndexOptions { Unique = true }));
        await PurchaseOrders.Indexes.CreateOneAsync(new CreateIndexModel<PurchaseOrder>(
            Builders<PurchaseOrder>.IndexKeys.Descending(po => po.OrderDate)));

        // StockTransfers
        await StockTransfers.Indexes.CreateOneAsync(new CreateIndexModel<StockTransfer>(
            Builders<StockTransfer>.IndexKeys.Ascending(st => st.CompanyId)));
        await StockTransfers.Indexes.CreateOneAsync(new CreateIndexModel<StockTransfer>(
            Builders<StockTransfer>.IndexKeys.Ascending(st => st.FromBranchId)));
        await StockTransfers.Indexes.CreateOneAsync(new CreateIndexModel<StockTransfer>(
            Builders<StockTransfer>.IndexKeys.Ascending(st => st.ToBranchId)));
        await StockTransfers.Indexes.CreateOneAsync(new CreateIndexModel<StockTransfer>(
            Builders<StockTransfer>.IndexKeys.Ascending(st => st.TransferNumber), new CreateIndexOptions { Unique = true }));
        await StockTransfers.Indexes.CreateOneAsync(new CreateIndexModel<StockTransfer>(
            Builders<StockTransfer>.IndexKeys.Descending(st => st.TransferDate)));

        // Users
        await Users.Indexes.CreateOneAsync(new CreateIndexModel<ApplicationUser>(
            Builders<ApplicationUser>.IndexKeys.Ascending(u => u.Email), new CreateIndexOptions { Unique = true }));
        await Users.Indexes.CreateOneAsync(new CreateIndexModel<ApplicationUser>(
            Builders<ApplicationUser>.IndexKeys.Ascending(u => u.NormalizedEmail)));
        await Users.Indexes.CreateOneAsync(new CreateIndexModel<ApplicationUser>(
            Builders<ApplicationUser>.IndexKeys.Ascending(u => u.CompanyId)));
        await Users.Indexes.CreateOneAsync(new CreateIndexModel<ApplicationUser>(
            Builders<ApplicationUser>.IndexKeys.Ascending(u => u.BranchId)));
    }
}