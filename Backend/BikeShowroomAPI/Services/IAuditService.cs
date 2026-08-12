using BikeShowroomAPI.DTOs;

namespace BikeShowroomAPI.Services;

public interface IAuditService
{
    /// <summary>Send an audit event to the AuditLogService microservice. Never throws.</summary>
    Task LogAsync(string entityType, string action, string? entityId = null, string? details = null, int? companyId = null, int? branchId = null);

    /// <summary>Read audit events from the microservice. Returns null if the service is unreachable.</summary>
    Task<List<AuditLogDTO>?> GetLogsAsync(string? entityType = null, string? action = null, DateTime? from = null, DateTime? to = null);

    /// <summary>Read a single audit event from the microservice. Returns null if not found or unreachable.</summary>
    Task<AuditLogDTO?> GetLogAsync(int id);
}
