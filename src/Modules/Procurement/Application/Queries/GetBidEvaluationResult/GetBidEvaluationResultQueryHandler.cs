using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetBidEvaluationResult;

public class GetBidEvaluationResultQueryHandler : IQueryHandler<GetBidEvaluationResultQuery, BidEvaluationDto?>
{
    private readonly IBidEvaluationRepository _evalRepo;
    private readonly IVendorRepository        _vendorRepo;

    public GetBidEvaluationResultQueryHandler(
        IBidEvaluationRepository evalRepo,
        IVendorRepository vendorRepo)
    {
        _evalRepo   = evalRepo;
        _vendorRepo = vendorRepo;
    }

    public async Task<BidEvaluationDto?> Handle(GetBidEvaluationResultQuery query, CancellationToken ct)
    {
        var evaluation = await _evalRepo.GetByRFQIdWithScoresAsync(query.RFQId, ct);
        if (evaluation is null) return null;

        var vendorIds = evaluation.Scores.Select(s => s.VendorId).Distinct().ToList();
        var vendors   = await _vendorRepo.GetByIdsAsync(vendorIds, ct);
        var vendorMap = vendors.ToDictionary(v => v.Id, v => v.LegalName);

        var scores = evaluation.Scores
            .OrderByDescending(s => s.WeightedTotal)
            .Select(s => new EvaluationScoreDto(
                s.QuotationId,
                s.VendorId,
                vendorMap.GetValueOrDefault(s.VendorId, string.Empty),
                s.PriceScore,
                s.QualityScore,
                s.DeliveryScore,
                s.WeightedTotal))
            .ToList();

        return new BidEvaluationDto(
            evaluation.Id,
            evaluation.RFQId,
            evaluation.PriceWeight,
            evaluation.QualityWeight,
            evaluation.DeliveryWeight,
            evaluation.Status,
            evaluation.AwardedVendorId,
            evaluation.AwardedQuotationId,
            scores);
    }
}
