using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.ConfirmPayment;

public record ConfirmPaymentCommand(
    Guid   InvoiceId,
    string PaymentReference
) : ICommand;
