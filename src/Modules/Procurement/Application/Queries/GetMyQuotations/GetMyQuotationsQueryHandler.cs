using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetMyQuotations;

public class GetMyQuotationsQueryHandler : IQueryHandler<GetMyQuotationsQuery, List<QuotationListDto>>
{
    private readonly IVendorQuotationRepository _quotationRepo;
    private readonly IRFQRepository             _rfqRepo;
    private readonly IVendorRepository          _vendorRepo;

    public GetMyQuotationsQueryHandler(
        IVendorQuotationRepository quotationRepo,
        IRFQRepository rfqRepo,
        IVendorRepository vendorRepo)
    {
        _quotationRepo = quotationRepo;
        _rfqRepo       = rfqRepo;
        _vendorRepo    = vendorRepo;
    }

    public async Task<List<QuotationListDto>> Handle(GetMyQuotationsQuery query, CancellationToken ct)
    {
        var quotations = await _quotationRepo.GetByVendorIdAsync(query.VendorId, ct);

        var rfqIds  = quotations.Select(q => q.RFQId).Distinct().ToList();
        var rfqData = new Dictionary<Guid, (string Number, string Title)>();
        foreach (var rfqId in rfqIds)
        {
            var rfq = await _rfqRepo.GetByIdAsync(rfqId, ct);
            if (rfq is not null)
                rfqData[rfqId] = (rfq.RFQNumber, rfq.Title);
        }

        var vendor    = await _vendorRepo.GetByIdAsync(query.VendorId, ct);
        var vendorName = vendor?.LegalName ?? string.Empty;

        return quotations.Select(q =>
        {
            rfqData.TryGetValue(q.RFQId, out var rfqInfo);
            return new QuotationListDto(
                q.Id,
                q.RFQId,
                rfqInfo.Number ?? string.Empty,
                rfqInfo.Title  ?? string.Empty,
                q.VendorId,
                vendorName,
                q.Status,
                q.TotalPrice,
                q.CreatedAt);
        }).ToList();
    }
}
