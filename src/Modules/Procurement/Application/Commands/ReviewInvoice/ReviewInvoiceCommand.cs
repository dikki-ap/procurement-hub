using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.ReviewInvoice;

public record ReviewInvoiceCommand(
    Guid    InvoiceId,
    Guid    ReviewerId,
    bool    Approve,
    string? RejectionReason
) : ICommand;
