using ProcureHub.Modules.VendorManagement.Application.DTOs;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorScoreHistory;

public class GetVendorScoreHistoryQueryHandler
    : IQueryHandler<GetVendorScoreHistoryQuery, List<VendorScoreDto>>
{
    private readonly IVendorScoreRepository _repo;

    public GetVendorScoreHistoryQueryHandler(IVendorScoreRepository repo) => _repo = repo;

    public async Task<List<VendorScoreDto>> Handle(
        GetVendorScoreHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var scores = await _repo.GetHistoryAsync(request.VendorId, cancellationToken);

        return scores.Select(s => new VendorScoreDto(
            s.Id,
            s.PeriodYear,
            s.PeriodQuarter,
            s.DeliveryScore,
            s.QualityScore,
            s.PriceScore,
            s.ResponseScore,
            s.DocScore,
            s.TotalScore,
            s.Tier?.ToString(),
            s.Notes,
            s.CalculatedAt
        )).ToList();
    }
}
