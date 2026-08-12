namespace AuditLogService.DTOs;

public class CreateAuditLogRequest
{
    public DateTime? Timestamp { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public int? CompanyId { get; set; }
    public int? BranchId { get; set; }
}
