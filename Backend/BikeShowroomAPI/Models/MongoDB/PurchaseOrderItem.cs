using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BikeShowroomAPI.Models.MongoDB;

public class PurchaseOrderItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("productId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProductId { get; set; } = string.Empty;

    [BsonElement("quantityOrdered")]
    public int QuantityOrdered { get; set; }

    [BsonElement("quantityReceived")]
    public int QuantityReceived { get; set; }

    [BsonElement("unitPrice")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal UnitPrice { get; set; }

    [BsonElement("totalPrice")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal TotalPrice { get; set; }
}