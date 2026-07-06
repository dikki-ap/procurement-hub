using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.Audit.Application.Commands.ExportAuditLog;
using ProcureHub.Modules.Audit.Application.Queries.GetAuditLog;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.Audit;

/// <summary>Audit trail — super_admin only.</summary>
[ApiController]
[Route("api/v1/audit")]
[Authorize(Policy = "RequireSuperAdmin")]
public class AuditLogController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogController(IMediator mediator) => _mediator = mediator;

    /// <summary>Query audit logs with filtering and pagination.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAuditLog(
        [FromQuery] string?   entityType = null,
        [FromQuery] Guid?     entityId   = null,
        [FromQuery] Guid?     userId     = null,
        [FromQuery] string?   action     = null,
        [FromQuery] DateTime? from       = null,
        [FromQuery] DateTime? to         = null,
        [FromQuery] int       page       = 1,
        [FromQuery] int       pageSize   = 50,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetAuditLogQuery(entityType, entityId, userId, action, from, to, page, pageSize), ct);

        return Ok(ApiResponse<PagedAuditLogDto>.Ok(result));
    }

    /// <summary>Export filtered audit logs as CSV stored in SeaweedFS, returns presigned URL.</summary>
    [HttpPost("export")]
    public async Task<IActionResult> ExportAuditLog(
        [FromBody] ExportAuditLogCommand command,
        CancellationToken ct)
    {
        var url = await _mediator.Send(command, ct);
        return Ok(ApiResponse<string>.Ok(url));
    }
}
