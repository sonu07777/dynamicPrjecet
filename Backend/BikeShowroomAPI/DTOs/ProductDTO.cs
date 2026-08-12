namespace BikeShowroomAPI.DTOs;

public class ProductDTO
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public string Unit { get; set; } = string.Empty;
    public int? MinStockLevel { get; set; }
    public int? MaxStockLevel { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public string? CategoryName { get; set; }
}

public class CreateProductDTO
{
    public int CompanyId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public string Unit { get; set; } = "pcs";
    public int? MinStockLevel { get; set; }
    public int? MaxStockLevel { get; set; }
    public string? ImageUrl { get; set; }
}

public class CategoryDTO
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ParentCategoryId { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCategoryDTO
{
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ParentCategoryId { get; set; }
}

public class InventoryDTO
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int BranchId { get; set; }
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity => Quantity - ReservedQuantity;
    public string? ProductName { get; set; }
    public string? BranchName { get; set; }
}

public class InventoryAdjustmentDTO
{
    public int ProductId { get; set; }
    public int BranchId { get; set; }
    public int Quantity { get; set; }
}
