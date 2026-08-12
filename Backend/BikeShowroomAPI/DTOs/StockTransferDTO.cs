namespace BikeShowroomAPI.DTOs;

public class StockTransferDTO
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int FromBranchId { get; set; }
    public int ToBranchId { get; set; }
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
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
}

public class CreateStockTransferDTO
{
    public int CompanyId { get; set; }
    public int FromBranchId { get; set; }
    public int ToBranchId { get; set; }
    public string? Notes { get; set; }
    public List<CreateStockTransferItemDTO> Items { get; set; } = new();
}

public class CreateStockTransferItemDTO
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
