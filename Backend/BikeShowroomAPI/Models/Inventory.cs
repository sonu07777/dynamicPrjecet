namespace BikeShowroomAPI.Models;

public class Inventory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int BranchId { get; set; }
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public DateTime LastRestocked { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
}
