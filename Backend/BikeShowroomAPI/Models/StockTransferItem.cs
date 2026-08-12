namespace BikeShowroomAPI.Models;

public class StockTransferItem
{
    public int Id { get; set; }
    public int StockTransferId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }

    public StockTransfer StockTransfer { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
