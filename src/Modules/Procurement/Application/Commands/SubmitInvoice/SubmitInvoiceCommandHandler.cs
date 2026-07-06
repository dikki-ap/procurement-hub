using MediatR;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.SubmitInvoice;

public class SubmitInvoiceCommandHandler : ICommandHandler<SubmitInvoiceCommand, Guid>
{
    private readonly IInvoiceRepository       _invoiceRepo;
    private readonly IPurchaseOrderRepository _poRepo;

    public SubmitInvoiceCommandHandler(
        IInvoiceRepository invoiceRepo,
        IPurchaseOrderRepository poRepo)
    {
        _invoiceRepo = invoiceRepo;
        _poRepo      = poRepo;
    }

    public async Task<Guid> Handle(SubmitInvoiceCommand command, CancellationToken ct)
    {
        var po = await _poRepo.GetByIdAsync(command.POId, ct)
                 ?? throw new NotFoundException("PurchaseOrder", command.POId);

        if (po.Status is not (Domain.Enums.POStatus.Acknowledged
                           or Domain.Enums.POStatus.InDelivery
                           or Domain.Enums.POStatus.Completed))
            throw new BusinessRuleException("InvoiceSubmit",
                "Invoice can only be submitted for acknowledged or completed POs.");

        var invoiceNumber = await _invoiceRepo.GenerateNextNumberAsync(ct);

        var invoice = Invoice.Create(
            invoiceNumber,
            command.POId,
            command.VendorId,
            command.Amount,
            command.TaxAmount,
            command.CurrencyId,
            command.FileUrl,
            command.DueAt,
            command.Notes);

        _invoiceRepo.Add(invoice);
        await _invoiceRepo.SaveChangesAsync(ct);

        return invoice.Id;
    }
}
