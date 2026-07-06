using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.SubmitInvoice;

public record SubmitInvoiceCommand(
    Guid      POId,
    Guid      VendorId,
    decimal   Amount,
    decimal   TaxAmount,
    Guid?     CurrencyId,
    string?   FileUrl,
    DateTime? DueAt,
    string?   Notes
) : ICommand<Guid>;
