using MediatR;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.ReviewInvoice;

public class ReviewInvoiceCommandHandler : ICommandHandler<ReviewInvoiceCommand>
{
    private readonly IInvoiceRepository _repo;

    public ReviewInvoiceCommandHandler(IInvoiceRepository repo) => _repo = repo;

    public async Task<Unit> Handle(ReviewInvoiceCommand command, CancellationToken ct)
    {
        var invoice = await _repo.GetByIdAsync(command.InvoiceId, ct)
                      ?? throw new NotFoundException("Invoice", command.InvoiceId);

        if (command.Approve)
            invoice.Approve(command.ReviewerId);
        else
        {
            if (string.IsNullOrWhiteSpace(command.RejectionReason))
                throw new BusinessRuleException("InvoiceReject", "Rejection reason is required.");
            invoice.Reject(command.ReviewerId, command.RejectionReason);
        }

        _repo.Update(invoice);
        await _repo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
