using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetRFQBids;

public class GetRFQBidsQueryHandler : IQueryHandler<GetRFQBidsQuery, List<QuotationListDto>>
{
    private readonly IVendorQuotationRepository _quotationRepo;
    private readonly IRFQRepository             _rfqRepo;
    private readonly IVendorRepository          _vendorRepo;

    public GetRFQBidsQueryHandler(
        IVendorQuotationRepository quotationRepo,
        IRFQRepository rfqRepo,
        IVendorRepository vendorRepo)
    {
        _quotationRepo = quotationRepo;
        _rfqRepo       = rfqRepo;
        _vendorRepo    = vendorRepo;
    }

    public async Task<List<QuotationListDto>> Handle(GetRFQBidsQuery query, CancellationToken ct)
    {
        var rfq        = await _rfqRepo.GetByIdAsync(query.RFQId, ct);
        var quotations = await _quotationRepo.GetByRFQIdAsync(query.RFQId, ct);

        var vendorIds = quotations.Select(q => q.VendorId).Distinct().ToList();
        var vendors   = await _vendorRepo.GetByIdsAsync(vendorIds, ct);
        var vendorMap = vendors.ToDictionary(v => v.Id, v => v.LegalName);

        return quotations.Select(q => new QuotationListDto(
            q.Id,
            q.RFQId,
            rfq?.RFQNumber ?? string.Empty,
            rfq?.Title     ?? string.Empty,
            q.VendorId,
            vendorMap.GetValueOrDefault(q.VendorId, string.Empty),
            q.Status,
            q.TotalPrice,
            q.CreatedAt)).ToList();
    }
}
