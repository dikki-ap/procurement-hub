using MediatR;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.ReviewInvoice;

public class ReviewInvoiceCommandHandler : ICommandHandler<ReviewInvoiceCommand>
{
    private readonly IInvoiceRepository      _invoiceRepo;
    private readonly IPurchaseOrderRepository _poRepo;

    public ReviewInvoiceCommandHandler(
        IInvoiceRepository      invoiceRepo,
        IPurchaseOrderRepository poRepo)
    {
        _invoiceRepo = invoiceRepo;
        _poRepo      = poRepo;
    }

    public async Task<Unit> Handle(ReviewInvoiceCommand command, CancellationToken ct)
    {
        var invoice = await _invoiceRepo.GetByIdAsync(command.InvoiceId, ct)
                      ?? throw new NotFoundException("Invoice", command.InvoiceId);

        if (command.Approve)
        {
            // Hard block: PO must exist and be in a valid status
            var po = await _poRepo.GetByIdAsync(invoice.POId, ct);
            if (po is null)
                throw new BusinessRuleException("InvoiceApprove", "Purchase order not found.");

            var validStatuses = new[] { POStatus.Issued, POStatus.Acknowledged, POStatus.InDelivery, POStatus.Completed };
            if (!validStatuses.Contains(po.Status))
                throw new BusinessRuleException("InvoiceApprove",
                    $"Cannot approve: PO status is '{po.Status}'. PO must be Issued or further along.");

            // Hard block: invoice total must not exceed PO total
            var siblings = await _invoiceRepo.GetByPOAsync(invoice.POId, ct);
            var alreadyInvoiced = siblings
                .Where(i => i.Id != invoice.Id && i.Status is InvoiceStatus.Approved or InvoiceStatus.Paid)
                .Sum(i => i.TotalAmount);

            if (invoice.TotalAmount > po.TotalAmount - alreadyInvoiced)
                throw new BusinessRuleException("InvoiceApprove",
                    $"Invoice total {invoice.TotalAmount:N2} exceeds remaining PO balance " +
                    $"{po.TotalAmount - alreadyInvoiced:N2}. Reduce invoice amount or reject.");

            invoice.Approve(command.ReviewerId);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(command.RejectionReason))
                throw new BusinessRuleException("InvoiceReject", "Rejection reason is required.");
            invoice.Reject(command.ReviewerId, command.RejectionReason);
        }

        _invoiceRepo.Update(invoice);
        await _invoiceRepo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
