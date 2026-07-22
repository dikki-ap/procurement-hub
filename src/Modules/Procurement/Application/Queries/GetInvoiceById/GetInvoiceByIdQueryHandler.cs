using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryHandler : IQueryHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    private readonly IInvoiceRepository      _invoiceRepo;
    private readonly IPurchaseOrderRepository _poRepo;
    private readonly IGoodsReceiptRepository  _grnRepo;

    public GetInvoiceByIdQueryHandler(
        IInvoiceRepository      invoiceRepo,
        IPurchaseOrderRepository poRepo,
        IGoodsReceiptRepository  grnRepo)
    {
        _invoiceRepo = invoiceRepo;
        _poRepo      = poRepo;
        _grnRepo     = grnRepo;
    }

    public async Task<InvoiceDto> Handle(GetInvoiceByIdQuery query, CancellationToken ct)
    {
        var invoice = await _invoiceRepo.GetByIdWithPOAsync(query.Id, ct)
                      ?? throw new NotFoundException("Invoice", query.Id);

        var po   = await _poRepo.GetByIdWithItemsAsync(invoice.POId, ct);
        var grns = await _grnRepo.GetByPOWithItemsAsync(invoice.POId, ct);

        var discrepancies = new List<string>();

        // 1 — PO check
        var validPoStatuses = new[] { POStatus.Issued, POStatus.Acknowledged, POStatus.InDelivery, POStatus.Completed };
        var poMatched = po is not null && validPoStatuses.Contains(po.Status);
        if (po is null)
            discrepancies.Add("Purchase order not found.");
        else if (!poMatched)
            discrepancies.Add($"PO status is '{po.Status}' — must be Issued, Acknowledged, InDelivery, or Completed.");

        // 2 — Amount check: invoice total ≤ PO total minus already-paid/approved invoices
        var amountMatched = false;
        if (po is not null)
        {
            var alreadyInvoiced = await _invoiceRepo.GetByPOAsync(invoice.POId, ct);
            var otherInvoiced   = alreadyInvoiced
                .Where(i => i.Id != invoice.Id && i.Status is InvoiceStatus.Approved or InvoiceStatus.Paid)
                .Sum(i => i.TotalAmount);

            var remaining = po.TotalAmount - otherInvoiced;
            amountMatched = invoice.TotalAmount <= remaining;
            if (!amountMatched)
                discrepancies.Add(
                    $"Invoice total {invoice.TotalAmount:N2} exceeds remaining PO amount {remaining:N2} " +
                    $"(PO: {po.TotalAmount:N2}, already invoiced: {otherInvoiced:N2}).");
        }

        // 3 — GRN check: confirmed received qty per POItem ≥ invoice amount implies delivery happened
        var grnMatched = false;
        if (grns.Any())
        {
            var confirmedGrns = grns.Where(g => g.Status != GRNStatus.Draft).ToList();
            if (!confirmedGrns.Any())
            {
                discrepancies.Add("No confirmed GRN found for this PO.");
            }
            else
            {
                // Per-item: total accepted received qty must cover the PO quantity
                if (po?.Items is not null)
                {
                    var grnItemsByPoItem = confirmedGrns
                        .SelectMany(g => g.Items)
                        .GroupBy(i => i.POItemId)
                        .ToDictionary(g => g.Key, g => g.Sum(i => i.ReceivedQty - i.RejectedQty));

                    var shortItems = po.Items
                        .Where(pi => {
                            var accepted = grnItemsByPoItem.TryGetValue(pi.Id, out var qty) ? qty : 0m;
                            return accepted < pi.Quantity;
                        })
                        .ToList();

                    grnMatched = !shortItems.Any();
                    foreach (var pi in shortItems)
                    {
                        var accepted = grnItemsByPoItem.TryGetValue(pi.Id, out var qty) ? qty : 0m;
                        discrepancies.Add(
                            $"Item '{pi.Description}': GRN accepted qty {accepted:N2} < ordered qty {pi.Quantity:N2}.");
                    }
                }
                else
                {
                    grnMatched = true;
                }
            }
        }
        else
        {
            discrepancies.Add("No GRN found for this PO.");
        }

        return new InvoiceDto(
            invoice.Id,
            invoice.InvoiceNumber,
            invoice.POId,
            po?.PONumber ?? string.Empty,
            invoice.VendorId,
            invoice.PurchaseOrder?.Vendor?.LegalName ?? string.Empty,
            invoice.Status,
            invoice.Amount,
            invoice.TaxAmount,
            invoice.TotalAmount,
            null,
            invoice.FileUrl,
            invoice.DueAt,
            invoice.PaidAt,
            invoice.PaymentReference,
            invoice.Notes,
            invoice.RejectionReason,
            invoice.SubmittedAt,
            invoice.ReviewedAt,
            poMatched,
            grnMatched,
            amountMatched,
            discrepancies);
    }
}
