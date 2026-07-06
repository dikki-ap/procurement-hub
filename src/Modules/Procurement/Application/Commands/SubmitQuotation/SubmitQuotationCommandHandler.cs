using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.SubmitQuotation;

public class SubmitQuotationCommandHandler : ICommandHandler<SubmitQuotationCommand, Guid>
{
    private readonly IVendorQuotationRepository _quotationRepo;
    private readonly IRFQRepository             _rfqRepo;

    public SubmitQuotationCommandHandler(
        IVendorQuotationRepository quotationRepo,
        IRFQRepository rfqRepo)
    {
        _quotationRepo = quotationRepo;
        _rfqRepo       = rfqRepo;
    }

    public async Task<Guid> Handle(SubmitQuotationCommand command, CancellationToken ct)
    {
        var rfq = await _rfqRepo.GetByIdWithDetailsAsync(command.RFQId, ct)
                  ?? throw new NotFoundException("RFQ", command.RFQId);

        if (rfq.Status != RFQStatus.Open)
            throw new BusinessRuleException("SubmitQuotation",
                $"Quotations can only be submitted for Open RFQs. Current status: {rfq.Status}");

        var isInvited = rfq.Vendors.Any(v => v.VendorId == command.VendorId
                                          && v.Status == RFQVendorStatus.Invited);
        if (!isInvited)
            throw new BusinessRuleException("SubmitQuotation", "Vendor is not invited to bid on this RFQ.");

        var existing = await _quotationRepo.GetByRFQAndVendorAsync(command.RFQId, command.VendorId, ct);
        if (existing is { Status: QuotationStatus.Submitted })
            throw new BusinessRuleException("SubmitQuotation", "Vendor has already submitted a quotation for this RFQ.");

        var quotation = existing ?? new VendorQuotation
        {
            RFQId    = command.RFQId,
            VendorId = command.VendorId,
            Notes    = command.Notes,
        };

        quotation.Notes = command.Notes;
        quotation.Items.Clear();

        foreach (var item in command.Items)
        {
            var rfqItem = rfq.Items.FirstOrDefault(i => i.Id == item.RFQItemId)
                          ?? throw new NotFoundException("RFQItem", item.RFQItemId);

            quotation.Items.Add(new QuotationItem
            {
                RFQItemId = item.RFQItemId,
                UnitPrice = item.UnitPrice,
                Quantity  = rfqItem.Quantity,
                Notes     = item.Notes,
            });
        }

        var rfqItemIds = rfq.Items.Select(i => i.Id).ToList();
        quotation.Submit(rfq.BidDeadline, rfqItemIds);

        if (existing is null)
            _quotationRepo.Add(quotation);
        else
            _quotationRepo.Update(quotation);

        await _quotationRepo.SaveChangesAsync(ct);
        return quotation.Id;
    }
}
