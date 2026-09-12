using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BikeShowroomAPI.Models.MongoDB;

public class StockTransfer
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("companyId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CompanyId { get; set; } = string.Empty;

    [BsonElement("fromBranchId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string FromBranchId { get; set; } = string.Empty;

    [BsonElement("toBranchId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ToBranchId { get; set; } = string.Empty;

    [BsonElement("transferNumber")]
    public string TransferNumber { get; set; } = string.Empty;

    [BsonElement("transferDate")]
    public DateTime TransferDate { get; set; } = DateTime.UtcNow;

    [BsonElement("status")]
    public string Status { get; set; } = "Pending";

    [BsonElement("notes")]
    public string? Notes { get; set; }

    [BsonElement("initiatedBy")]
    public string InitiatedBy { get; set; } = string.Empty;

    [BsonElement("items")]
    public List<StockTransferItem> Items { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}