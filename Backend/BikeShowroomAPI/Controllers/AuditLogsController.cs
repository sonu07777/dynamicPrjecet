using BikeShowroomAPI.DTOs;
using BikeShowroomAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BikeShowroomAPI.Controllers;

// Gateway to the AuditLogService microservice.
// The frontend always talks to this API (:5000); this controller proxies reads
// to the microservice so the UI never needs to know its address.
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,CompanyAdmin")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditLogsController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLogDTO>>> GetAll(
        [FromQuery] string? entityType = null,
        [FromQuery] string? action = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var logs = await _auditService.GetLogsAsync(entityType, action, from, to);
        if (logs == null)
            return StatusCode(502, new { message = "Audit log service is unavailable" });

        return Ok(logs);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuditLogDTO>> GetById(int id)
    {
        var log = await _auditService.GetLogAsync(id);
        if (log == null)
            return NotFound();

        return Ok(log);
    }
}
