using BikeShowroomAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BikeShowroomAPI.Filters;

/// <summary>
/// Automatically records an audit event for every successful write (POST/PUT/DELETE)
/// by forwarding the event to the AuditLogService microservice.
/// Never captures request bodies, so sensitive data (e.g. login passwords) is never logged.
/// Failures are swallowed — audit logging must never break the business request.
/// </summary>
public class AuditActionFilter : IAsyncActionFilter
{
    private readonly IAuditService _auditService;

    public AuditActionFilter(IAuditService auditService)
    {
        _auditService = auditService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpMethod = context.HttpContext.Request.Method;
        var path = context.HttpContext.Request.Path.Value ?? string.Empty;

        // Only write operations, and never the audit or auth endpoints themselves.
        // Auth is excluded because login happens without a token (nothing to attribute the audit to).
        if (httpMethod is not ("POST" or "PUT" or "DELETE") ||
            path.StartsWith("/api/auditlogs", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/api/auth", StringComparison.OrdinalIgnoreCase))
        {
            await next();
            return;
        }

        var executed = await next();

        if (!IsSuccess(executed))
            return;

        try
        {
            var controller = context.RouteData.Values["controller"]?.ToString() ?? string.Empty;
            var entityType = controller.EndsWith("Controller", StringComparison.Ordinal)
                ? controller[..^"Controller".Length]
                : controller;

            var action = httpMethod switch
            {
                "POST" => "Create",
                "PUT" => "Update",
                "DELETE" => "Delete",
                _ => "Write"
            };

            var entityId = context.RouteData.Values["id"]?.ToString();
            var companyId = ParseRouteInt("companyId", context);
            var branchId = ParseRouteInt("branchId", context);

            await _auditService.LogAsync(entityType, action, entityId, companyId: companyId, branchId: branchId);
        }
        catch
        {
            // Last-resort guard; AuditService already logs its own failures.
        }
    }

    private static bool IsSuccess(ActionExecutedContext context)
    {
        if (context.Exception != null)
            return false;

        return context.Result switch
        {
            OkResult or OkObjectResult or CreatedResult or CreatedAtActionResult or CreatedAtRouteResult or NoContentResult => true,
            ObjectResult r when r.StatusCode is >= 200 and <= 299 => true,
            _ => false
        };
    }

    private static int? ParseRouteInt(string key, ActionExecutingContext context)
    {
        if (context.RouteData.Values.TryGetValue(key, out var raw) &&
            raw is string value &&
            int.TryParse(value, out var parsed))
        {
            return parsed;
        }
        return null;
    }
}
