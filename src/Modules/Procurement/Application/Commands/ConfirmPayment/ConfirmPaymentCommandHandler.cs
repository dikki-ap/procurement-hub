using MediatR;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.ConfirmPayment;

public class ConfirmPaymentCommandHandler : ICommandHandler<ConfirmPaymentCommand>
{
    private readonly IInvoiceRepository _repo;

    public ConfirmPaymentCommandHandler(IInvoiceRepository repo) => _repo = repo;

    public async Task<Unit> Handle(ConfirmPaymentCommand command, CancellationToken ct)
    {
        var invoice = await _repo.GetByIdAsync(command.InvoiceId, ct)
                      ?? throw new NotFoundException("Invoice", command.InvoiceId);

        invoice.ConfirmPayment(command.PaymentReference);
        _repo.Update(invoice);
        await _repo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
