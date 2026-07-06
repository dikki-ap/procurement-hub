using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.CreateGRN;

public record CreateGRNCommand(
    Guid               POId,
    Guid               ReceivedBy,
    string?            DeliveryNoteNo,
    string?            Notes,
    List<GRNItemInput> Items
) : ICommand<Guid>;

public record GRNItemInput(
    Guid          POItemId,
    decimal       ReceivedQty,
    decimal       RejectedQty,
    QualityStatus QualityStatus,
    string?       RejectReason,
    string?       Notes
);
