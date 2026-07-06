using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.Procurement.Application.Commands.AcknowledgePO;
using ProcureHub.Modules.Procurement.Application.Commands.CreatePO;
using ProcureHub.Modules.Procurement.Application.Commands.IssuePO;
using ProcureHub.Modules.Procurement.Application.Queries.GetPOById;
using ProcureHub.Modules.Procurement.Application.Queries.GetPOList;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.Procurement;

/// <summary>Purchase Order management.</summary>
[ApiController]
[Route("api/v1/purchase-orders")]
[Authorize(Policy = "RequireInternal")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IMediator           _mediator;
    private readonly ICurrentUserService _currentUser;

    public PurchaseOrdersController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
    }

    /// <summary>List POs for a company.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetList(
        [FromQuery] Guid companyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPOListQuery(companyId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get PO detail by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPOByIdQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Create a new PO (draft).</summary>
    [HttpPost]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> Create(
        [FromBody] CreatePOCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse.Ok(id));
    }

    /// <summary>Issue (print PDF) an approved PO.</summary>
    [HttpPost("{id:guid}/issue")]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> Issue(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new IssuePOCommand(id), ct);
        return Ok(ApiResponse.Ok("PO issued."));
    }

    /// <summary>Vendor acknowledges receipt of PO.</summary>
    [HttpPost("{id:guid}/acknowledge")]
    public async Task<ActionResult<ApiResponse<object>>> Acknowledge(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new AcknowledgePOCommand(id), ct);
        return Ok(ApiResponse.Ok("PO acknowledged."));
    }
}
