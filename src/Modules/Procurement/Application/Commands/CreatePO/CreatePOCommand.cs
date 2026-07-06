using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.CreatePO;

public record CreatePOCommand(
    Guid              CompanyId,
    Guid              VendorId,
    Guid?             RFQId,
    Guid?             CurrencyId,
    Guid?             PaymentTermId,
    Guid?             DeliveryLocationId,
    DateTime?         ExpectedDelivery,
    string?           Notes,
    string?           TermsConditions,
    List<POItemInput> Items
) : ICommand<Guid>;

public record POItemInput(
    Guid?   MaterialId,
    string  Description,
    decimal Quantity,
    Guid?   UomId,
    decimal UnitPrice
);
