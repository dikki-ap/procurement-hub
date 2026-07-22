using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.CreateReturnOrder;

public record CreateReturnOrderItemRequest(
    Guid?   POItemId,
    string  ItemDescription,
    decimal Quantity,
    string  Uom,
    string? ReturnReason
);

public record CreateReturnOrderCommand(
    Guid                               GRNId,
    Guid                               VendorId,
    string?                            Reason,
    string?                            Notes,
    List<CreateReturnOrderItemRequest> Items
) : ICommand<Guid>;
