namespace BikeShowroomAPI.DTOs;

public class SaleDTO
{
    public string Id { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string PaymentStatus { get; set; } = "Paid";
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }
    public string? Notes { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? BranchName { get; set; }
    public string CashierId { get; set; } = string.Empty;
    public List<SaleItemDTO> Items { get; set; } = new();
    public List<PaymentDTO> Payments { get; set; } = new();
}

public class SaleItemDTO
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPrice { get; set; }
}

public class PaymentDTO
{
    public string Id { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}

public class CreateSaleDTO
{
    public string CompanyId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string? Notes { get; set; }
    public List<CreateSaleItemDTO> Items { get; set; } = new();
}

public class CreateSaleItemDTO
{
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
}

public class SalesStatsDTO
{
    public decimal TodaySales { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal MonthSales { get; set; }
    public decimal MonthRevenue { get; set; }
    public decimal AverageSale { get; set; }
}
