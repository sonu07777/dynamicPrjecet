using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BikeShowroomAPI.Models.MongoDB;

public class ApplicationUser
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("userName")]
    public string UserName { get; set; } = string.Empty;

    [BsonElement("normalizedUserName")]
    public string NormalizedUserName { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("normalizedEmail")]
    public string NormalizedEmail { get; set; } = string.Empty;

    [BsonElement("emailConfirmed")]
    public bool EmailConfirmed { get; set; }

    [BsonElement("passwordHash")]
    public string? PasswordHash { get; set; }

    [BsonElement("securityStamp")]
    public string SecurityStamp { get; set; } = string.Empty;

    [BsonElement("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [BsonElement("phoneNumberConfirmed")]
    public bool PhoneNumberConfirmed { get; set; }

    [BsonElement("twoFactorEnabled")]
    public bool TwoFactorEnabled { get; set; }

    [BsonElement("lockoutEnd")]
    public DateTimeOffset? LockoutEnd { get; set; }

    [BsonElement("lockoutEnabled")]
    public bool LockoutEnabled { get; set; }

    [BsonElement("accessFailedCount")]
    public int AccessFailedCount { get; set; }

    [BsonElement("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [BsonElement("lastName")]
    public string LastName { get; set; } = string.Empty;

    [BsonElement("companyId")]
    [BsonIgnoreIfNull]
    public string? CompanyId { get; set; }

    [BsonElement("branchId")]
    [BsonIgnoreIfNull]
    public string? BranchId { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("lastLoginAt")]
    public DateTime? LastLoginAt { get; set; }

    [BsonElement("roles")]
    public List<string> Roles { get; set; } = new();
}