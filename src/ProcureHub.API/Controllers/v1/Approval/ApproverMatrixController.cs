using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.ApprovalEngine.Application.Commands.CreateApproverMatrixEntry;
using ProcureHub.Modules.ApprovalEngine.Application.Commands.DeleteApproverMatrixEntry;
using ProcureHub.Modules.ApprovalEngine.Application.Commands.UpdateApproverMatrixEntry;
using ProcureHub.Modules.ApprovalEngine.Application.DTOs;
using ProcureHub.Modules.ApprovalEngine.Application.Queries.GetApproverMatrix;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Common;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.API.Controllers.v1.Approval;

/// <summary>Manage the per-company approver matrix (who approves PR/PO/RFQ at each level).</summary>
[ApiController]
[Route("api/v1/approver-matrix")]
[Authorize(Policy = "RequireSuperAdmin")]
public class ApproverMatrixController : ControllerBase
{
    private readonly IMediator               _mediator;
    private readonly ICurrentUserService     _currentUser;
    private readonly IApproverMatrixRepository _matrixRepo;
    private readonly ApplicationDbContext    _db;

    public ApproverMatrixController(
        IMediator mediator,
        ICurrentUserService currentUser,
        IApproverMatrixRepository matrixRepo,
        ApplicationDbContext db)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
        _matrixRepo  = matrixRepo;
        _db          = db;
    }

    /// <summary>List all approver matrix entries for a company.</summary>
    [HttpGet]
    [Authorize(Policy = "RequireInternal")]
    public async Task<ActionResult<ApiResponse<List<ApproverMatrixEntryDto>>>> GetAll(
        [FromQuery] Guid companyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetApproverMatrixQuery(companyId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Create a new approver matrix entry.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> Create(
        [FromBody] CreateApproverMatrixEntryRequest request, CancellationToken ct)
    {
        var id = await _mediator.Send(
            new CreateApproverMatrixEntryCommand(
                request.CompanyId,
                request.ReferenceType,
                request.Level,
                request.Name,
                request.Position,
                request.Email), ct);

        return Ok(ApiResponse.Ok(id));
    }

    /// <summary>Update an existing approver matrix entry.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        Guid id, [FromBody] UpdateApproverMatrixEntryRequest request, CancellationToken ct)
    {
        await _mediator.Send(
            new UpdateApproverMatrixEntryCommand(
                id,
                request.ReferenceType,
                request.Level,
                request.Name,
                request.Position,
                request.Email), ct);

        return Ok(ApiResponse.Ok("Approver matrix entry updated."));
    }

    /// <summary>Delete an approver matrix entry.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteApproverMatrixEntryCommand(id), ct);
        return Ok(ApiResponse.Ok("Approver matrix entry deleted."));
    }

    /// <summary>Check whether the currently authenticated user is configured as an approver for their company.</summary>
    [HttpGet("am-i-approver")]
    [Authorize(Policy = "RequireInternal")]
    public async Task<ActionResult<ApiResponse<object>>> AmIApprover(CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue) return Unauthorized();

        var email = _currentUser.Email;
        if (string.IsNullOrWhiteSpace(email)) return Unauthorized();

        var companyId = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId.Value)
            .Select(u => u.CompanyId)
            .FirstOrDefaultAsync(ct);

        if (companyId == Guid.Empty)
            return NotFound();

        var isApprover = await _matrixRepo.IsApproverAsync(companyId, email, ct);
        return Ok(ApiResponse.Ok(new { isApprover }));
    }
}

public record CreateApproverMatrixEntryRequest(
    Guid   CompanyId,
    string ReferenceType,
    int    Level,
    string Name,
    string Position,
    string Email);

public record UpdateApproverMatrixEntryRequest(
    string ReferenceType,
    int    Level,
    string Name,
    string Position,
    string Email);
