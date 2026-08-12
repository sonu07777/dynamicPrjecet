using System.Net.Http.Json;
using BikeShowroomAPI.DTOs;
using Microsoft.AspNetCore.WebUtilities;

namespace BikeShowroomAPI.Services;

/// <summary>
/// Typed HttpClient wrapper for the AuditLogService microservice.
/// Forwards the caller's JWT so the microservice can validate it and attribute audits.
/// All failures are swallowed and logged — audit logging must never break a business request.
/// </summary>
public class AuditService : IAuditService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditService> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(string entityType, string action, string? entityId = null, string? details = null, int? companyId = null, int? branchId = null)
    {
        try
        {
            var payload = new CreateAuditLogDTO
            {
                Timestamp = DateTime.UtcNow,
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                Details = details,
                CompanyId = companyId,
                BranchId = branchId
            };

            using var message = new HttpRequestMessage(HttpMethod.Post, "/api/auditlogs")
            {
                Content = JsonContent.Create(payload)
            };
            ForwardAuthorization(message);

            var response = await _httpClient.SendAsync(message);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("AuditLogService returned {StatusCode} for {EntityType}.{Action}", response.StatusCode, entityType, action);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send audit event to AuditLogService");
        }
    }

    public async Task<List<AuditLogDTO>?> GetLogsAsync(string? entityType = null, string? action = null, DateTime? from = null, DateTime? to = null)
    {
        try
        {
            var query = new Dictionary<string, string?>();
            if (!string.IsNullOrWhiteSpace(entityType)) query["entityType"] = entityType;
            if (!string.IsNullOrWhiteSpace(action)) query["action"] = action;
            if (from.HasValue) query["from"] = from.Value.ToString("O");
            if (to.HasValue) query["to"] = to.Value.ToString("O");

            var url = QueryHelpers.AddQueryString("/api/auditlogs", query);
            using var message = new HttpRequestMessage(HttpMethod.Get, url);
            ForwardAuthorization(message);

            var response = await _httpClient.SendAsync(message);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<List<AuditLogDTO>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read audit logs from AuditLogService");
            return null;
        }
    }

    public async Task<AuditLogDTO?> GetLogAsync(int id)
    {
        try
        {
            using var message = new HttpRequestMessage(HttpMethod.Get, $"/api/auditlogs/{id}");
            ForwardAuthorization(message);

            var response = await _httpClient.SendAsync(message);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<AuditLogDTO>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read audit log {Id} from AuditLogService", id);
            return null;
        }
    }

    // Forward the caller's Bearer token so the microservice can authorize the request.
    private void ForwardAuthorization(HttpRequestMessage message)
    {
        var auth = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(auth))
        {
            message.Headers.TryAddWithoutValidation("Authorization", auth);
        }
    }
}
