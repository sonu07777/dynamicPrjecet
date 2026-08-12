namespace BikeShowroomAPI.Models;

public class Sale
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public int BranchId { get; set; }
    public int? CustomerId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = "Cash"; // Cash, Card, BankTransfer, Multiple
    public string PaymentStatus { get; set; } = "Paid"; // Paid, Pending, PartiallyPaid
    public decimal AmountPaid { get; set; }
    public decimal AmountDue { get; set; }
    public string? Notes { get; set; }
    public string CashierId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Company Company { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public Customer? Customer { get; set; }
    public ApplicationUser Cashier { get; set; } = null!;
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
