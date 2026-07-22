using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProcureHub.Modules.Procurement.Application.Commands.SubmitQuotation;
using ProcureHub.Modules.Procurement.Application.Commands.UploadQuotationAttachment;
using ProcureHub.Modules.Procurement.Application.Commands.WithdrawQuotation;
using ProcureHub.Modules.Procurement.Application.Queries.GetMyQuotations;
using ProcureHub.Modules.Procurement.Application.Queries.GetQuotationAttachmentUrl;
using ProcureHub.Modules.VendorManagement.Application.Queries.GetMyVendorId;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Common;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.API.Controllers.v1.Procurement;

/// <summary>Vendor quotation submission and management.</summary>
[ApiController]
[Route("api/v1/quotations")]
[Authorize(Policy = "RequireVendor")]
public class QuotationsController : ControllerBase
{
    private readonly IMediator                _mediator;
    private readonly ICurrentUserService      _currentUser;
    private readonly IDocumentAccessLogger    _accessLogger;

    public QuotationsController(IMediator mediator, ICurrentUserService currentUser, IDocumentAccessLogger accessLogger)
    {
        _mediator     = mediator;
        _currentUser  = currentUser;
        _accessLogger = accessLogger;
    }

    /// <summary>List all quotations for the authenticated vendor.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetMyQuotations(
        [FromQuery] Guid vendorId, CancellationToken ct)
    {
        var myVendorId = await ResolveVendorIdAsync(ct);
        if (myVendorId != vendorId)
            return Forbid();

        var result = await _mediator.Send(new GetMyQuotationsQuery(vendorId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Submit a quotation for an open RFQ.</summary>
    [HttpPost]
    [EnableRateLimiting("bid-submit")]
    public async Task<ActionResult<ApiResponse<object>>> Submit(
        [FromBody] SubmitQuotationRequest request, CancellationToken ct)
    {
        var myVendorId = await ResolveVendorIdAsync(ct);

        var id = await _mediator.Send(new SubmitQuotationCommand(
            request.RFQId, myVendorId, request.Notes, request.Items), ct);
        return Ok(ApiResponse.Ok(id));
    }

    /// <summary>Withdraw a previously submitted quotation.</summary>
    [HttpPost("{id:guid}/withdraw")]
    public async Task<ActionResult<ApiResponse<object>>> Withdraw(Guid id, CancellationToken ct)
    {
        var myVendorId = await ResolveVendorIdAsync(ct);
        await _mediator.Send(new WithdrawQuotationCommand(id, myVendorId), ct);
        return Ok(ApiResponse.Ok("Quotation withdrawn."));
    }

    /// <summary>Upload a technical document attachment for a quotation (replaces existing).</summary>
    [HttpPost("{id:guid}/upload")]
    public async Task<ActionResult<ApiResponse<object>>> Upload(
        Guid id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("No file provided."));

        using var ms = new MemoryStream((int)file.Length);
        await file.CopyToAsync(ms, ct);
        ms.Position = 0;

        var key = await _mediator.Send(
            new UploadQuotationAttachmentCommand(id, ms, file.FileName, file.ContentType), ct);
        return Ok(ApiResponse.Ok(new { key }));
    }

    /// <summary>Get a 30-minute presigned download URL for a quotation attachment.</summary>
    [HttpGet("{id:guid}/download")]
    public async Task<ActionResult<ApiResponse<object>>> Download(Guid id, CancellationToken ct)
    {
        var url = await _mediator.Send(new GetQuotationAttachmentUrlQuery(id), ct);
        await _accessLogger.LogAsync("QuotationAttachment", id, null, false, ct);
        return Ok(ApiResponse.Ok(new { url }));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task<Guid> ResolveVendorIdAsync(CancellationToken ct)
    {
        var keycloakId = _currentUser.KeycloakId;
        if (string.IsNullOrEmpty(keycloakId))
            throw new ForbiddenException();

        return await _mediator.Send(new GetMyVendorIdQuery(keycloakId), ct);
    }
}

public record SubmitQuotationRequest(
    Guid                          RFQId,
    string?                       Notes,
    List<SubmitQuotationItemInput> Items);
