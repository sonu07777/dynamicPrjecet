namespace BikeShowroomAPI.DTOs;

// Read model returned by the AuditLogService microservice.
public class AuditLogDTO
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
}

// Write payload sent to the AuditLogService microservice.
public class CreateAuditLogDTO
{
    public DateTime? Timestamp { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
}
