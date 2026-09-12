namespace BikeShowroomAPI.DTOs;

public class StockTransferDTO
{
    public string Id { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public string FromBranchId { get; set; } = string.Empty;
    public string ToBranchId { get; set; } = string.Empty;
    public string TransferNumber { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string InitiatedBy { get; set; } = string.Empty;
    public string? FromBranchName { get; set; }
    public string? ToBranchName { get; set; }
    public List<StockTransferItemDTO> Items { get; set; } = new();
}

public class StockTransferItemDTO
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
}

public class CreateStockTransferDTO
{
    public string CompanyId { get; set; } = string.Empty;
    public string FromBranchId { get; set; } = string.Empty;
    public string ToBranchId { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<CreateStockTransferItemDTO> Items { get; set; } = new();
}

public class CreateStockTransferItemDTO
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
