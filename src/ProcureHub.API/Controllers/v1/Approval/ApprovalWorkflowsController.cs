using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.ApprovalEngine.Application.Commands.Approve;
using ProcureHub.Modules.ApprovalEngine.Application.Commands.DelegateApproval;
using ProcureHub.Modules.ApprovalEngine.Application.Commands.Reject;
using ProcureHub.Modules.ApprovalEngine.Application.Commands.Revise;
using ProcureHub.Modules.ApprovalEngine.Application.Commands.SubmitForApproval;
using ProcureHub.Modules.ApprovalEngine.Application.Queries.GetApprovalInbox;
using ProcureHub.Modules.ApprovalEngine.Application.Queries.GetApprovalWorkflow;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.Approval;

/// <summary>Approval workflow management for multi-level approvals.</summary>
[ApiController]
[Route("api/v1/approval-workflows")]
[Authorize(Policy = "RequireInternal")]
public class ApprovalWorkflowsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApprovalWorkflowsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get the approver's inbox — pending workflows assigned to them.</summary>
    [HttpGet("inbox")]
    [Authorize(Policy = "RequireApprover")]
    public async Task<ActionResult<ApiResponse<object>>> GetInbox(
        [FromQuery] Guid userId, [FromQuery] Guid companyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetApprovalInboxQuery(userId, companyId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get full workflow detail including history and assignments.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetApprovalWorkflowQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Submit a document for approval. Creates a new workflow.</summary>
    [HttpPost]
    [Authorize(Policy = "RequireCreatePR")]
    public async Task<ActionResult<ApiResponse<object>>> Submit(
        [FromBody] SubmitForApprovalCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse.Ok(id));
    }

    /// <summary>Approve the current level of a workflow.</summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "RequireApprover")]
    public async Task<ActionResult<ApiResponse<object>>> Approve(
        Guid id, [FromBody] ApproveActionRequest request, CancellationToken ct)
    {
        await _mediator.Send(
            new ApproveCommand(id, request.ApproverId, request.ApproverName), ct);
        return Ok(ApiResponse.Ok("Approved successfully."));
    }

    /// <summary>Soft-reject: send back to previous level (or Revised if at level 1).</summary>
    [HttpPost("{id:guid}/revise")]
    [Authorize(Policy = "RequireApprover")]
    public async Task<ActionResult<ApiResponse<object>>> Revise(
        Guid id, [FromBody] ReviseActionRequest request, CancellationToken ct)
    {
        await _mediator.Send(
            new ReviseCommand(id, request.ApproverId, request.ApproverName, request.Reason), ct);
        return Ok(ApiResponse.Ok("Revision requested."));
    }

    /// <summary>Hard-reject: rejects the document entirely.</summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "RequireApprover")]
    public async Task<ActionResult<ApiResponse<object>>> Reject(
        Guid id, [FromBody] RejectActionRequest request, CancellationToken ct)
    {
        await _mediator.Send(
            new RejectCommand(id, request.ApproverId, request.ApproverName, request.Reason), ct);
        return Ok(ApiResponse.Ok("Rejected."));
    }

    /// <summary>Delegate current-level approval to another user.</summary>
    [HttpPost("{id:guid}/delegate")]
    [Authorize(Policy = "RequireApprover")]
    public async Task<ActionResult<ApiResponse<object>>> Delegate(
        Guid id, [FromBody] DelegateActionRequest request, CancellationToken ct)
    {
        await _mediator.Send(
            new DelegateApprovalCommand(id, request.ApproverId, request.ApproverName,
                request.DelegateToUserId, request.DelegateToUserName), ct);
        return Ok(ApiResponse.Ok("Approval delegated."));
    }
}

public record ApproveActionRequest(Guid ApproverId, string ApproverName);
public record ReviseActionRequest(Guid ApproverId, string ApproverName, string Reason);
public record RejectActionRequest(Guid ApproverId, string ApproverName, string Reason);
public record DelegateActionRequest(Guid ApproverId, string ApproverName, Guid DelegateToUserId, string DelegateToUserName);
