using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BikeShowroomAPI.Models.MongoDB;

public class PurchaseOrder
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

    [BsonElement("supplierId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string SupplierId { get; set; } = string.Empty;

    [BsonElement("orderNumber")]
    public string OrderNumber { get; set; } = string.Empty;

    [BsonElement("orderDate")]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [BsonElement("expectedDeliveryDate")]
    public DateTime? ExpectedDeliveryDate { get; set; }

    [BsonElement("receivedDate")]
    public DateTime? ReceivedDate { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "Pending";

    [BsonElement("totalAmount")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalAmount { get; set; }

    [BsonElement("notes")]
    public string? Notes { get; set; }

    [BsonElement("createdBy")]
    public string CreatedBy { get; set; } = string.Empty;

    [BsonElement("items")]
    public List<PurchaseOrderItem> Items { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}