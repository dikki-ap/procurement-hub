using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProcureHub.Modules.Procurement.Application.Commands.SubmitQuotation;
using ProcureHub.Modules.Procurement.Application.Commands.WithdrawQuotation;
using ProcureHub.Modules.Procurement.Application.Queries.GetMyQuotations;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.Procurement;

/// <summary>Vendor quotation submission and management.</summary>
[ApiController]
[Route("api/v1/quotations")]
[Authorize(Policy = "RequireVendor")]
public class QuotationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public QuotationsController(IMediator mediator) => _mediator = mediator;

    /// <summary>List all quotations for the authenticated vendor.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetMyQuotations(
        [FromQuery] Guid vendorId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMyQuotationsQuery(vendorId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Submit a quotation for an open RFQ.</summary>
    [HttpPost]
    [EnableRateLimiting("bid-submit")]
    public async Task<ActionResult<ApiResponse<object>>> Submit(
        [FromBody] SubmitQuotationCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return Ok(ApiResponse.Ok(id));
    }

    /// <summary>Withdraw a previously submitted quotation.</summary>
    [HttpPost("{id:guid}/withdraw")]
    public async Task<ActionResult<ApiResponse<object>>> Withdraw(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new WithdrawQuotationCommand(id), ct);
        return Ok(ApiResponse.Ok("Quotation withdrawn."));
    }
}
