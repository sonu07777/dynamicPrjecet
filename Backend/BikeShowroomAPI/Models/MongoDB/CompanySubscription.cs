using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BikeShowroomAPI.Models.MongoDB;

public class CompanySubscription
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("companyId")]
    public string CompanyId { get; set; } = string.Empty;

    [BsonElement("planId")]
    public string PlanId { get; set; } = string.Empty;

    [BsonElement("razorpaySubscriptionId")]
    public string RazorpaySubscriptionId { get; set; } = string.Empty;

    [BsonElement("lastPaymentId")]
    public string? LastPaymentId { get; set; }

    [BsonElement("status")]
    public string Status { get; set; } = "created";

    [BsonElement("cancelAtCycleEnd")]
    public bool CancelAtCycleEnd { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}