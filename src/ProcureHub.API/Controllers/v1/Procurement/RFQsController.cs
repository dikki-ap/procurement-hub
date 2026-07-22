using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.Procurement.Application.Commands.AssignEvaluator;
using ProcureHub.Modules.Procurement.Application.Commands.AwardVendor;
using ProcureHub.Modules.Procurement.Application.Commands.CloseRFQ;
using ProcureHub.Modules.Procurement.Application.Commands.CreateRFQ;
using ProcureHub.Modules.Procurement.Application.Commands.EvaluateBids;
using ProcureHub.Modules.Procurement.Application.Commands.FinalizeEvaluation;
using ProcureHub.Modules.Procurement.Application.Commands.InviteVendors;
using ProcureHub.Modules.Procurement.Application.Commands.OpenRFQ;
using ProcureHub.Modules.Procurement.Application.Commands.SubmitEvaluatorScore;
using ProcureHub.Modules.Procurement.Application.Commands.UploadRFQAttachment;
using ProcureHub.Modules.Procurement.Application.Queries.GetBidComparison;
using ProcureHub.Modules.Procurement.Application.Queries.GetBidEvaluationResult;
using ProcureHub.Modules.Procurement.Application.Queries.GetRFQAttachmentUrl;
using ProcureHub.Modules.Procurement.Application.Queries.GetRFQBids;
using ProcureHub.Modules.Procurement.Application.Queries.GetRFQById;
using ProcureHub.Modules.Procurement.Application.Queries.GetRFQList;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.Procurement;

/// <summary>Request for Quotation management.</summary>
[ApiController]
[Route("api/v1/rfqs")]
[Authorize(Policy = "RequireInternal")]
public class RFQsController : ControllerBase
{
    private readonly IMediator                _mediator;
    private readonly ICurrentUserService      _currentUser;
    private readonly IDocumentAccessLogger    _accessLogger;

    public RFQsController(IMediator mediator, ICurrentUserService currentUser, IDocumentAccessLogger accessLogger)
    {
        _mediator     = mediator;
        _currentUser  = currentUser;
        _accessLogger = accessLogger;
    }

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

    // ── Multi-Evaluator Bid Evaluation ────────────────────────────────────────

    /// <summary>Assign a user as an evaluator for this RFQ's bid evaluation.</summary>
    [HttpPost("{id:guid}/evaluators")]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> AssignEvaluator(
        Guid id, [FromBody] AssignEvaluatorRequest request, CancellationToken ct)
    {
        var assignmentId = await _mediator.Send(
            new AssignEvaluatorCommand(id, request.UserId, request.UserName), ct);
        return Ok(ApiResponse.Ok(new { assignmentId }));
    }

    /// <summary>Submit per-evaluator scores for all quotations.</summary>
    [HttpPost("{id:guid}/evaluators/scores")]
    [Authorize(Policy = "RequireInternal")]
    public async Task<ActionResult<ApiResponse<object>>> SubmitEvaluatorScores(
        Guid id, [FromBody] SubmitScoresRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new InvalidOperationException("Could not resolve internal user ID.");

        await _mediator.Send(new SubmitEvaluatorScoreCommand(
            id, userId,
            request.Scores.Select(s => new EvaluatorScoreInput(s.QuotationId, s.QualityScore, s.DeliveryScore)).ToList()),
            ct);
        return Ok(ApiResponse.Ok("Scores submitted."));
    }

    /// <summary>Finalize evaluation — averages all evaluator scores, prepares for award.</summary>
    [HttpPost("{id:guid}/finalize")]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> FinalizeEvaluation(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new FinalizeEvaluationCommand(id), ct);
        return Ok(ApiResponse.Ok("Evaluation finalized."));
    }

    /// <summary>Upload TOR/spec attachment for an RFQ (replaces existing).</summary>
    [HttpPost("{id:guid}/upload")]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> Upload(
        Guid id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("No file provided."));

        using var ms = new MemoryStream((int)file.Length);
        await file.CopyToAsync(ms, ct);
        ms.Position = 0;

        var key = await _mediator.Send(
            new UploadRFQAttachmentCommand(id, ms, file.FileName, file.ContentType), ct);
        return Ok(ApiResponse.Ok(new { key }));
    }

    /// <summary>Get a 30-minute presigned download URL for the RFQ attachment.</summary>
    [HttpGet("{id:guid}/download")]
    public async Task<ActionResult<ApiResponse<object>>> Download(Guid id, CancellationToken ct)
    {
        var url = await _mediator.Send(new GetRFQAttachmentUrlQuery(id), ct);
        await _accessLogger.LogAsync("RFQAttachment", id, null, false, ct);
        return Ok(ApiResponse.Ok(new { url }));
    }
}

public record AssignEvaluatorRequest(Guid UserId, string UserName);
public record SubmitScoresRequest(List<ScoreInputRequest> Scores);
public record ScoreInputRequest(Guid QuotationId, decimal QualityScore, decimal DeliveryScore);
