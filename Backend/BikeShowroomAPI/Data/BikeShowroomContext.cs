using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BikeShowroomAPI.Models;

namespace BikeShowroomAPI.Data;

public class BikeShowroomContext : IdentityDbContext<ApplicationUser>
{
    public BikeShowroomContext(DbContextOptions<BikeShowroomContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    public DbSet<StockTransfer> StockTransfers { get; set; }
    public DbSet<StockTransferItem> StockTransferItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Company relationships
        modelBuilder.Entity<Company>()
            .HasMany(c => c.Branches)
            .WithOne(b => b.Company)
            .HasForeignKey(b => b.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Company>()
            .HasMany(c => c.Users)
            .WithOne(u => u.Company)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Branch relationships
        modelBuilder.Entity<Branch>()
            .HasMany(b => b.Users)
            .WithOne(u => u.Branch)
            .HasForeignKey(u => u.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Category relationships
        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Product relationships
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Company)
            .WithMany()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Inventory relationships
        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.Product)
            .WithMany(p => p.Inventories)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Inventory>()
            .HasOne(i => i.Branch)
            .WithMany(b => b.Inventories)
            .HasForeignKey(i => i.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // Sale relationships
        modelBuilder.Entity<Sale>()
            .HasOne(s => s.Company)
            .WithMany()
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Sale>()
            .HasOne(s => s.Branch)
            .WithMany(b => b.Sales)
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Sale>()
            .HasOne(s => s.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Sale>()
            .HasOne(s => s.Cashier)
            .WithMany()
            .HasForeignKey(s => s.CashierId)
            .OnDelete(DeleteBehavior.Restrict);

        // SaleItem relationships
        modelBuilder.Entity<SaleItem>()
            .HasOne(si => si.Sale)
            .WithMany(s => s.Items)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SaleItem>()
            .HasOne(si => si.Product)
            .WithMany(p => p.SaleItems)
            .HasForeignKey(si => si.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Payment relationships
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Sale)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        // PurchaseOrder relationships
        modelBuilder.Entity<PurchaseOrder>()
            .HasOne(po => po.Company)
            .WithMany()
            .HasForeignKey(po => po.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseOrder>()
            .HasOne(po => po.Branch)
            .WithMany()
            .HasForeignKey(po => po.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PurchaseOrder>()
            .HasOne(po => po.Supplier)
            .WithMany(s => s.PurchaseOrders)
            .HasForeignKey(po => po.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // PurchaseOrderItem relationships
        modelBuilder.Entity<PurchaseOrderItem>()
            .HasOne(poi => poi.PurchaseOrder)
            .WithMany(po => po.Items)
            .HasForeignKey(poi => poi.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PurchaseOrderItem>()
            .HasOne(poi => poi.Product)
            .WithMany(p => p.PurchaseOrderItems)
            .HasForeignKey(poi => poi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // StockTransfer relationships
        modelBuilder.Entity<StockTransfer>()
            .HasOne(st => st.Company)
            .WithMany()
            .HasForeignKey(st => st.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockTransfer>()
            .HasOne(st => st.FromBranch)
            .WithMany()
            .HasForeignKey(st => st.FromBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StockTransfer>()
            .HasOne(st => st.ToBranch)
            .WithMany()
            .HasForeignKey(st => st.ToBranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // StockTransferItem relationships
        modelBuilder.Entity<StockTransferItem>()
            .HasOne(sti => sti.StockTransfer)
            .WithMany(st => st.Items)
            .HasForeignKey(sti => sti.StockTransferId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StockTransferItem>()
            .HasOne(sti => sti.Product)
            .WithMany()
            .HasForeignKey(sti => sti.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Customer relationships
        modelBuilder.Entity<Customer>()
            .HasOne(c => c.Company)
            .WithMany()
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Supplier relationships
        modelBuilder.Entity<Supplier>()
            .HasOne(s => s.Company)
            .WithMany()
            .HasForeignKey(s => s.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Category relationships
        modelBuilder.Entity<Category>()
            .HasOne(c => c.Company)
            .WithMany()
            .HasForeignKey(c => c.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Decimal precision
        modelBuilder.Entity<Product>()
            .Property(p => p.CostPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Product>()
            .Property(p => p.SellingPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Sale>()
            .Property(s => s.SubTotal)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Sale>()
            .Property(s => s.TaxAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Sale>()
            .Property(s => s.DiscountAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Sale>()
            .Property(s => s.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Sale>()
            .Property(s => s.AmountPaid)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Sale>()
            .Property(s => s.AmountDue)
            .HasPrecision(18, 2);

        modelBuilder.Entity<SaleItem>()
            .Property(si => si.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<SaleItem>()
            .Property(si => si.Discount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<SaleItem>()
            .Property(si => si.TotalPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PurchaseOrder>()
            .Property(po => po.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PurchaseOrderItem>()
            .Property(poi => poi.UnitPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PurchaseOrderItem>()
            .Property(poi => poi.TotalPrice)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Customer>()
            .Property(c => c.CreditLimit)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Customer>()
            .Property(c => c.CurrentBalance)
            .HasPrecision(18, 2);

        // Indexes for better performance
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.SKU)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Barcode);

        modelBuilder.Entity<Sale>()
            .HasIndex(s => s.InvoiceNumber)
            .IsUnique();

        modelBuilder.Entity<PurchaseOrder>()
            .HasIndex(po => po.OrderNumber)
            .IsUnique();

        modelBuilder.Entity<StockTransfer>()
            .HasIndex(st => st.TransferNumber)
            .IsUnique();
    }
}
