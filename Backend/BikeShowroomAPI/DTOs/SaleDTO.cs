namespace BikeShowroomAPI.DTOs;

public class SaleDTO
{
    public int Id { get; set; }
    public int BikeId { get; set; }
    public int CustomerId { get; set; }
    public decimal SalePrice { get; set; }
    public DateTime SaleDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public BikeDTO? Bike { get; set; }
    public CustomerDTO? Customer { get; set; }
}

public class CreateSaleDTO
{
    public int BikeId { get; set; }
    public int CustomerId { get; set; }
    public decimal SalePrice { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = "Completed";
    public string Notes { get; set; } = string.Empty;
}
