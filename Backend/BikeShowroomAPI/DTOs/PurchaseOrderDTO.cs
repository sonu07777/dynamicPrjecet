namespace BikeShowroomAPI.DTOs;

public class PurchaseOrderDTO
{
    public string Id { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string SupplierId { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? SupplierName { get; set; }
    public string? BranchName { get; set; }
    public List<PurchaseOrderItemDTO> Items { get; set; } = new();
}

public class PurchaseOrderItemDTO
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string? ProductName { get; set; }
    public int QuantityOrdered { get; set; }
    public int QuantityReceived { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreatePurchaseOrderDTO
{
    public string CompanyId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string SupplierId { get; set; } = string.Empty;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string? Notes { get; set; }
    public List<CreatePurchaseOrderItemDTO> Items { get; set; } = new();
}

public class CreatePurchaseOrderItemDTO
{
    public string ProductId { get; set; } = string.Empty;
    public int QuantityOrdered { get; set; }
    public decimal UnitPrice { get; set; }
}
