using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BikeShowroomAPI.Models.MongoDB;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("companyId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CompanyId { get; set; } = string.Empty;

    [BsonElement("categoryId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CategoryId { get; set; } = string.Empty;

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("sku")]
    public string SKU { get; set; } = string.Empty;

    [BsonElement("barcode")]
    public string Barcode { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("costPrice")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal CostPrice { get; set; }

    [BsonElement("sellingPrice")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal SellingPrice { get; set; }

    [BsonElement("unit")]
    public string Unit { get; set; } = "pcs";

    [BsonElement("minStockLevel")]
    public int? MinStockLevel { get; set; }

    [BsonElement("maxStockLevel")]
    public int? MaxStockLevel { get; set; }

    [BsonElement("imageUrl")]
    public string? ImageUrl { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}