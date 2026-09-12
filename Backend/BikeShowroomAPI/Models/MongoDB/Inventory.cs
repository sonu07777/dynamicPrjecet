using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BikeShowroomAPI.Models.MongoDB;

public class Inventory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("productId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ProductId { get; set; } = string.Empty;

    [BsonElement("branchId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string BranchId { get; set; } = string.Empty;

    [BsonElement("quantity")]
    public int Quantity { get; set; }

    [BsonElement("reservedQuantity")]
    public int ReservedQuantity { get; set; }

    [BsonElement("lastRestocked")]
    public DateTime LastRestocked { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}