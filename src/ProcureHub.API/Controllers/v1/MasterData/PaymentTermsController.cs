using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.MasterData.Application.Commands.CreatePaymentTerm;
using ProcureHub.Modules.MasterData.Application.Commands.DeletePaymentTerm;
using ProcureHub.Modules.MasterData.Application.Commands.UpdatePaymentTerm;
using ProcureHub.Modules.MasterData.Application.Queries.GetPaymentTermById;
using ProcureHub.Modules.MasterData.Application.Queries.GetPaymentTermList;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.MasterData;

/// <summary>Payment Terms master data — readable by all internal users, writable by super_admin only.</summary>
[ApiController]
[Route("api/v1/master-data/payment-terms")]
[Authorize(Policy = "RequireInternal")]
public class PaymentTermsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentTermsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get all payment terms for a company.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetList(
        [FromQuery] Guid companyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPaymentTermListQuery(companyId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get payment term by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPaymentTermByIdQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Create a new payment term.</summary>
    [HttpPost]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Create(
        [FromBody] CreatePaymentTermCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse.Ok(new { id }));
    }

    /// <summary>Update a payment term.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        Guid id, [FromBody] UpdatePaymentTermCommand command, CancellationToken ct)
    {
        await _mediator.Send(command with { Id = id }, ct);
        return Ok(ApiResponse.Ok("Payment term updated."));
    }

    /// <summary>Delete a payment term.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeletePaymentTermCommand(id), ct);
        return Ok(ApiResponse.Ok("Payment term deleted."));
    }
}
