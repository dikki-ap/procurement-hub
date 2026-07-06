using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.Procurement.Application.Commands.ConfirmGRN;
using ProcureHub.Modules.Procurement.Application.Commands.CreateGRN;
using ProcureHub.Modules.Procurement.Application.Queries.GetGRNById;
using ProcureHub.Modules.Procurement.Application.Queries.GetGRNList;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.Procurement;

/// <summary>Goods Receipt Note management.</summary>
[ApiController]
[Route("api/v1/goods-receipts")]
[Authorize(Policy = "RequireInternal")]
public class GoodsReceiptsController : ControllerBase
{
    private readonly IMediator           _mediator;
    private readonly ICurrentUserService _currentUser;

    public GoodsReceiptsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
    }

    /// <summary>List GRNs for a PO.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetList(
        [FromQuery] Guid poId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetGRNListQuery(poId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get GRN detail by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetGRNByIdQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Create a GRN (draft).</summary>
    [HttpPost]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> Create(
        [FromBody] CreateGRNCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse.Ok(id));
    }

    /// <summary>Confirm a GRN (triggers vendor score update).</summary>
    [HttpPost("{id:guid}/confirm")]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> Confirm(
        Guid id, [FromQuery] Guid vendorId, CancellationToken ct)
    {
        await _mediator.Send(new ConfirmGRNCommand(id, vendorId), ct);
        return Ok(ApiResponse.Ok("GRN confirmed."));
    }
}
