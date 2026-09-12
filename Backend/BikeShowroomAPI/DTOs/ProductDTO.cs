namespace BikeShowroomAPI.DTOs;

public class ProductDTO
{
    public string Id { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
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
    public string CompanyId { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
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
    public string Id { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ParentCategoryId { get; set; }
    public bool IsActive { get; set; }
}

public class CreateCategoryDTO
{
    public string CompanyId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ParentCategoryId { get; set; }
}

public class InventoryDTO
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity => Quantity - ReservedQuantity;
    public string? ProductName { get; set; }
    public string? BranchName { get; set; }
}

public class InventoryAdjustmentDTO
{
    public string ProductId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class CreateInventoryDTO
{
    public string ProductId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class UpdateInventoryDTO
{
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
}
