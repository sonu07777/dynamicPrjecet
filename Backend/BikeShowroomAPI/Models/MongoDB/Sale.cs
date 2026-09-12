using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BikeShowroomAPI.Models.MongoDB;

public class Sale
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("companyId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CompanyId { get; set; } = string.Empty;

    [BsonElement("branchId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string BranchId { get; set; } = string.Empty;

    [BsonElement("customerId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? CustomerId { get; set; }

    [BsonElement("invoiceNumber")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [BsonElement("saleNumber")]
    public string SaleNumber { get; set; } = string.Empty;

    [BsonElement("saleDate")]
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    [BsonElement("status")]
    public string Status { get; set; } = "Completed";

    [BsonElement("subTotal")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal SubTotal { get; set; }

    [BsonElement("taxAmount")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal TaxAmount { get; set; }

    [BsonElement("discountAmount")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal DiscountAmount { get; set; }

    [BsonElement("totalAmount")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalAmount { get; set; }

    [BsonElement("paymentMethod")]
    public string PaymentMethod { get; set; } = "Cash";

    [BsonElement("paymentStatus")]
    public string PaymentStatus { get; set; } = "Paid";

    [BsonElement("amountPaid")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal AmountPaid { get; set; }

    [BsonElement("amountDue")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal AmountDue { get; set; }

    [BsonElement("notes")]
    public string? Notes { get; set; }

    [BsonElement("createdBy")]
    public string CreatedBy { get; set; } = string.Empty;

    [BsonElement("items")]
    public List<SaleItem> Items { get; set; } = new();

    [BsonElement("payments")]
    public List<Payment> Payments { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}