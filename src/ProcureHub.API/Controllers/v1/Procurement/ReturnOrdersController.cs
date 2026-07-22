using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.Procurement.Application.Commands.AcknowledgeReturnOrder;
using ProcureHub.Modules.Procurement.Application.Commands.ConfirmReturnReceived;
using ProcureHub.Modules.Procurement.Application.Commands.CreateReturnOrder;
using ProcureHub.Modules.Procurement.Application.Queries.GetReturnOrders;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Common;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.API.Controllers.v1.Procurement;

/// <summary>Return Order management.</summary>
[ApiController]
[Route("api/v1/return-orders")]
[Authorize(Policy = "RequirePurchasing")]
public class ReturnOrdersController : ControllerBase
{
    private readonly IMediator              _mediator;
    private readonly ICurrentUserService    _currentUser;
    private readonly IPurchaseOrderRepository _poRepo;
    private readonly IGoodsReceiptRepository  _grnRepo;

    public ReturnOrdersController(
        IMediator mediator,
        ICurrentUserService currentUser,
        IPurchaseOrderRepository poRepo,
        IGoodsReceiptRepository  grnRepo)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
        _poRepo      = poRepo;
        _grnRepo     = grnRepo;
    }

    /// <summary>List all return orders for the company.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetList(
        [FromQuery] Guid companyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetReturnOrdersQuery(companyId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Create a return order from a confirmed GRN.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> Create(
        [FromBody] CreateReturnOrderRequest request, CancellationToken ct)
    {
        var grn = await _grnRepo.GetByIdAsync(request.GRNId, ct)
            ?? throw new NotFoundException("GoodsReceipt", request.GRNId);

        var po = await _poRepo.GetByIdAsync(grn.POId, ct)
            ?? throw new NotFoundException("PurchaseOrder", grn.POId);

        var id = await _mediator.Send(new CreateReturnOrderCommand(
            grn.Id, po.VendorId, request.Reason, request.Notes,
            request.Items.Select(i => new CreateReturnOrderItemRequest(
                i.POItemId, i.ItemDescription, i.Quantity, i.Uom, i.ReturnReason)).ToList()), ct);

        return CreatedAtAction(nameof(GetList), ApiResponse.Ok(new { id }));
    }

    /// <summary>Confirm return goods received (internal).</summary>
    [HttpPost("{id:guid}/received")]
    public async Task<ActionResult<ApiResponse<object>>> ConfirmReceived(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ConfirmReturnReceivedCommand(id), ct);
        return Ok(ApiResponse.Ok("Return confirmed as received."));
    }
}

public record CreateReturnOrderRequest(
    Guid                           GRNId,
    string?                        Reason,
    string?                        Notes,
    List<ReturnOrderItemRequest>   Items
);

public record ReturnOrderItemRequest(
    Guid?   POItemId,
    string  ItemDescription,
    decimal Quantity,
    string  Uom,
    string? ReturnReason
);
