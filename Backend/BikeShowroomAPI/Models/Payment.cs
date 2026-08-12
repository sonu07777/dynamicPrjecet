namespace BikeShowroomAPI.Models;

public class Payment
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public Sale Sale { get; set; } = null!;
}
