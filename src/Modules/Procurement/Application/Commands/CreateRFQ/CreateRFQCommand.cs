using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.CreateRFQ;

public record CreateRFQItemRequest(
    Guid?   PRItemId,
    Guid?   MaterialId,
    string  ItemDescription,
    decimal Quantity,
    Guid?   UnitOfMeasureId,
    string? UnitLabel);

public record CreateRFQCommand(
    Guid                        CompanyId,
    string                      Title,
    Guid?                       PurchaseRequisitionId,
    DateTime                    BidDeadline,
    DateTime?                   DeliveryDate,
    string?                     Notes,
    string?                     Terms,
    List<CreateRFQItemRequest>  Items) : ICommand<Guid>;
