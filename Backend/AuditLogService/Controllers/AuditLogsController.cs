using System.Security.Claims;
using AuditLogService.Data;
using AuditLogService.DTOs;
using AuditLogService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuditLogService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly AuditLogDbContext _context;

    public AuditLogsController(AuditLogDbContext context)
    {
        _context = context;
    }

    // Health check for connectivity probing (anonymous).
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health() => Ok(new { status = "ok" });

    // Record an audit event. Called by the main API on behalf of an authenticated user.
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateAuditLogRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.EntityType) || string.IsNullOrWhiteSpace(request.Action))
            return BadRequest("EntityType and Action are required.");

        var log = new AuditLog
        {
            Timestamp = request.Timestamp ?? DateTime.UtcNow,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            UserEmail = User.FindFirstValue(ClaimTypes.Email),
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Action = request.Action,
            Details = request.Details,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            CompanyId = request.CompanyId ?? ParseClaimInt("CompanyId"),
            BranchId = request.BranchId ?? ParseClaimInt("BranchId")
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();

        return Ok(log);
    }

    // List audit events, newest first, with optional filters.
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? entityType,
        [FromQuery] string? action,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(x => x.EntityType == entityType);
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(x => x.Action == action);
        if (from.HasValue)
            query = query.Where(x => x.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(x => x.Timestamp <= to.Value);

        var logs = await query.OrderByDescending(x => x.Timestamp).ToListAsync();
        return Ok(logs);
    }

    // Get a single audit event.
    [HttpGet("{id:int}")]
    [Authorize(Roles = "SuperAdmin,CompanyAdmin")]
    public async Task<IActionResult> GetById(int id)
    {
        var log = await _context.AuditLogs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (log == null)
            return NotFound();

        return Ok(log);
    }

    private int? ParseClaimInt(string claimType)
    {
        var value = User.FindFirstValue(claimType);
        return int.TryParse(value, out var parsed) ? parsed : null;
    }
}
