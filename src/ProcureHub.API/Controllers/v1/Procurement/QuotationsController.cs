using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProcureHub.Modules.Procurement.Application.Commands.SubmitQuotation;
using ProcureHub.Modules.Procurement.Application.Commands.WithdrawQuotation;
using ProcureHub.Modules.Procurement.Application.Queries.GetMyQuotations;
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
    private readonly IMediator           _mediator;
    private readonly ICurrentUserService _currentUser;

    public QuotationsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
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
