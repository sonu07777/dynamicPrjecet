using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BikeShowroomAPI.Models.MongoDB;

public class SubscriptionPlan
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("amount")]
    public decimal Amount { get; set; }

    [BsonElement("period")]
    public string Period { get; set; } = "monthly";

    [BsonElement("interval")]
    public int Interval { get; set; } = 1;

    [BsonElement("features")]
    public List<string> Features { get; set; } = [];

    [BsonElement("razorpayPlanId")]
    public string RazorpayPlanId { get; set; } = string.Empty;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}