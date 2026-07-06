using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.Procurement.Application.Commands.ConfirmPayment;
using ProcureHub.Modules.Procurement.Application.Commands.ReviewInvoice;
using ProcureHub.Modules.Procurement.Application.Commands.SubmitInvoice;
using ProcureHub.Modules.Procurement.Application.Queries.GetInvoiceById;
using ProcureHub.Modules.Procurement.Application.Queries.GetInvoiceList;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.Procurement;

/// <summary>Invoice management.</summary>
[ApiController]
[Route("api/v1/invoices")]
[Authorize(Policy = "RequireInternal")]
public class InvoicesController : ControllerBase
{
    private readonly IMediator           _mediator;
    private readonly ICurrentUserService _currentUser;

    public InvoicesController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
    }

    /// <summary>List all invoices (finance only).</summary>
    [HttpGet]
    [Authorize(Policy = "RequireFinance")]
    public async Task<ActionResult<ApiResponse<object>>> GetList(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetInvoiceListQuery(), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get invoice detail by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetInvoiceByIdQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Submit an invoice (vendor).</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> Submit(
        [FromBody] SubmitInvoiceCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse.Ok(id));
    }

    /// <summary>Review an invoice — approve or reject (finance).</summary>
    [HttpPost("{id:guid}/review")]
    [Authorize(Policy = "RequireFinance")]
    public async Task<ActionResult<ApiResponse<object>>> Review(
        Guid id, [FromBody] ReviewRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ReviewInvoiceCommand(id, _currentUser.UserId ?? Guid.Empty, request.Approve, request.RejectionReason), ct);
        return Ok(ApiResponse.Ok(request.Approve ? "Invoice approved." : "Invoice rejected."));
    }

    /// <summary>Confirm payment for an approved invoice (finance).</summary>
    [HttpPost("{id:guid}/confirm-payment")]
    [Authorize(Policy = "RequireFinance")]
    public async Task<ActionResult<ApiResponse<object>>> ConfirmPayment(
        Guid id, [FromBody] ConfirmPaymentRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ConfirmPaymentCommand(id, request.PaymentReference), ct);
        return Ok(ApiResponse.Ok("Payment confirmed."));
    }

    public record ReviewRequest(bool Approve, string? RejectionReason);
    public record ConfirmPaymentRequest(string PaymentReference);
}
