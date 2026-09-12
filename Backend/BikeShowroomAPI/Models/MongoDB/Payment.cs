using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BikeShowroomAPI.Models.MongoDB;

public class Payment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("paymentMethod")]
    public string PaymentMethod { get; set; } = string.Empty;

    [BsonElement("amount")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Amount { get; set; }

    [BsonElement("referenceNumber")]
    public string? ReferenceNumber { get; set; }

    [BsonElement("paymentDate")]
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [BsonElement("notes")]
    public string? Notes { get; set; }
}