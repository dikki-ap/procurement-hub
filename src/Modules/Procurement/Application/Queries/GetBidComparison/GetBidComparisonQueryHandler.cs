using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetBidComparison;

public class GetBidComparisonQueryHandler : IQueryHandler<GetBidComparisonQuery, BidComparisonDto>
{
    private readonly IRFQRepository             _rfqRepo;
    private readonly IVendorQuotationRepository _quotationRepo;
    private readonly IVendorRepository          _vendorRepo;

    public GetBidComparisonQueryHandler(
        IRFQRepository rfqRepo,
        IVendorQuotationRepository quotationRepo,
        IVendorRepository vendorRepo)
    {
        _rfqRepo       = rfqRepo;
        _quotationRepo = quotationRepo;
        _vendorRepo    = vendorRepo;
    }

    public async Task<BidComparisonDto> Handle(GetBidComparisonQuery query, CancellationToken ct)
    {
        var rfq = await _rfqRepo.GetByIdWithDetailsAsync(query.RFQId, ct)
                  ?? throw new NotFoundException("RFQ", query.RFQId);

        var quotations = await _quotationRepo.GetByRFQIdAsync(query.RFQId, ct);

        var vendorIds = quotations.Select(q => q.VendorId).Distinct().ToList();
        var vendors   = await _vendorRepo.GetByIdsAsync(vendorIds, ct);
        var vendorMap = vendors.ToDictionary(v => v.Id, v => v.LegalName);

        var items = rfq.Items.Select(i => new BidComparisonItemDto(
            i.Id,
            i.ItemDescription,
            i.Quantity,
            i.UnitLabel)).ToList();

        var rows = quotations.Select(q =>
        {
            var itemPrices = rfq.Items.Select(rfqItem =>
            {
                var qItem = q.Items.FirstOrDefault(qi => qi.RFQItemId == rfqItem.Id);
                return new BidComparisonPriceDto(
                    rfqItem.Id,
                    qItem?.UnitPrice ?? 0,
                    qItem != null ? qItem.UnitPrice * qItem.Quantity : 0,
                    qItem?.Notes);
            }).ToList();

            return new BidComparisonRowDto(
                q.VendorId,
                vendorMap.GetValueOrDefault(q.VendorId, string.Empty),
                q.Id,
                q.Status.ToString(),
                q.TotalPrice,
                itemPrices);
        }).ToList();

        return new BidComparisonDto(rfq.Id, rfq.RFQNumber, rfq.Title, items, rows);
    }
}
