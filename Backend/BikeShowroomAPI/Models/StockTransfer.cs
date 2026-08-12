namespace BikeShowroomAPI.Models;

public class StockTransfer
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int FromBranchId { get; set; }
    public int ToBranchId { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending"; // Pending, InTransit, Received, Cancelled
    public string? Notes { get; set; }
    public string InitiatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Company Company { get; set; } = null!;
    public Branch FromBranch { get; set; } = null!;
    public Branch ToBranch { get; set; } = null!;
    public ICollection<StockTransferItem> Items { get; set; } = new List<StockTransferItem>();
}
