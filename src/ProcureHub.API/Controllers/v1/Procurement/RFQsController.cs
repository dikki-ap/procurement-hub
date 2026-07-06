using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.Procurement.Application.Commands.AwardVendor;
using ProcureHub.Modules.Procurement.Application.Commands.CloseRFQ;
using ProcureHub.Modules.Procurement.Application.Commands.CreateRFQ;
using ProcureHub.Modules.Procurement.Application.Commands.EvaluateBids;
using ProcureHub.Modules.Procurement.Application.Commands.InviteVendors;
using ProcureHub.Modules.Procurement.Application.Commands.OpenRFQ;
using ProcureHub.Modules.Procurement.Application.Queries.GetBidComparison;
using ProcureHub.Modules.Procurement.Application.Queries.GetBidEvaluationResult;
using ProcureHub.Modules.Procurement.Application.Queries.GetRFQBids;
using ProcureHub.Modules.Procurement.Application.Queries.GetRFQById;
using ProcureHub.Modules.Procurement.Application.Queries.GetRFQList;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.Procurement;

/// <summary>Request for Quotation management.</summary>
[ApiController]
[Route("api/v1/rfqs")]
[Authorize(Policy = "RequireInternal")]
public class RFQsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RFQsController(IMediator mediator) => _mediator = mediator;

    /// <summary>List all RFQs for a company.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetList(
        [FromQuery] Guid companyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRFQListQuery(companyId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get RFQ detail by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRFQByIdQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Create a new RFQ (draft).</summary>
    [HttpPost]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> Create(
        [FromBody] CreateRFQCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse.Ok(id));
    }

    /// <summary>Invite vendors to bid on an RFQ.</summary>
    [HttpPost("{id:guid}/invite-vendors")]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> InviteVendors(
        Guid id, [FromBody] List<Guid> vendorIds, CancellationToken ct)
    {
        await _mediator.Send(new InviteVendorsCommand(id, vendorIds), ct);
        return Ok(ApiResponse.Ok("Vendors invited successfully."));
    }

    /// <summary>Open an RFQ for bidding (requires minimum 3 vendors).</summary>
    [HttpPost("{id:guid}/open")]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> Open(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new OpenRFQCommand(id), ct);
        return Ok(ApiResponse.Ok("RFQ opened for bidding."));
    }

    /// <summary>Close an RFQ and stop accepting bids.</summary>
    [HttpPost("{id:guid}/close")]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> Close(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new CloseRFQCommand(id), ct);
        return Ok(ApiResponse.Ok("RFQ closed."));
    }

    /// <summary>List all submitted quotations (bids) for an RFQ.</summary>
    [HttpGet("{id:guid}/bids")]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> GetBids(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRFQBidsQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get the bid comparison matrix for an RFQ.</summary>
    [HttpGet("{id:guid}/bids/comparison")]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> GetBidComparison(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBidComparisonQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Submit weighted evaluation scores for all bids.</summary>
    [HttpPost("{id:guid}/evaluate")]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> Evaluate(
        Guid id, [FromBody] EvaluateBidsCommand command, CancellationToken ct)
    {
        var evalId = await _mediator.Send(command with { RFQId = id }, ct);
        return Ok(ApiResponse.Ok(evalId));
    }

    /// <summary>Get the bid evaluation result for an RFQ.</summary>
    [HttpGet("{id:guid}/evaluation")]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> GetEvaluationResult(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBidEvaluationResultQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Award the bid to a selected vendor.</summary>
    [HttpPost("{id:guid}/award")]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> Award(
        Guid id, [FromBody] AwardVendorCommand command, CancellationToken ct)
    {
        await _mediator.Send(command with { RFQId = id }, ct);
        return Ok(ApiResponse.Ok("Bid awarded successfully."));
    }
}
