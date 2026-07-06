using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.Procurement.Application.Commands.CancelPR;
using ProcureHub.Modules.Procurement.Application.Commands.CreatePR;
using ProcureHub.Modules.Procurement.Application.Commands.SubmitPR;
using ProcureHub.Modules.Procurement.Application.Queries.GetPRById;
using ProcureHub.Modules.Procurement.Application.Queries.GetPRList;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.Procurement;

/// <summary>Purchase Requisition management.</summary>
[ApiController]
[Route("api/v1/purchase-requisitions")]
[Authorize(Policy = "RequireInternal")]
public class PurchaseRequisitionsController : ControllerBase
{
    private readonly IMediator           _mediator;
    private readonly ICurrentUserService _currentUser;

    public PurchaseRequisitionsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
    }

    /// <summary>List all PRs for a company.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetList(
        [FromQuery] Guid companyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPRListQuery(companyId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get PR detail by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPRByIdQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Create a new Purchase Requisition (draft).</summary>
    [HttpPost]
    [Authorize(Policy = "RequireCreatePR")]
    public async Task<ActionResult<ApiResponse<object>>> Create(
        [FromBody] CreatePRCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse.Ok(id));
    }

    /// <summary>Submit a draft PR for approval workflow.</summary>
    [HttpPost("{id:guid}/submit")]
    [Authorize(Policy = "RequireCreatePR")]
    public async Task<ActionResult<ApiResponse<object>>> Submit(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new SubmitPRCommand(id), ct);
        return Ok(ApiResponse.Ok("PR submitted successfully."));
    }

    /// <summary>Cancel a PR (draft or submitted only).</summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "RequireCreatePR")]
    public async Task<ActionResult<ApiResponse<object>>> Cancel(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new CancelPRCommand(id), ct);
        return Ok(ApiResponse.Ok("PR cancelled."));
    }
}
