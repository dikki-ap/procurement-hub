using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.EvaluateBids;

public class EvaluateBidsCommandHandler : ICommandHandler<EvaluateBidsCommand, Guid>
{
    private readonly IBidEvaluationRepository   _evalRepo;
    private readonly IVendorQuotationRepository _quotationRepo;
    private readonly IRFQRepository             _rfqRepo;

    public EvaluateBidsCommandHandler(
        IBidEvaluationRepository evalRepo,
        IVendorQuotationRepository quotationRepo,
        IRFQRepository rfqRepo)
    {
        _evalRepo      = evalRepo;
        _quotationRepo = quotationRepo;
        _rfqRepo       = rfqRepo;
    }

    public async Task<Guid> Handle(EvaluateBidsCommand command, CancellationToken ct)
    {
        var rfq = await _rfqRepo.GetByIdAsync(command.RFQId, ct)
                  ?? throw new NotFoundException("RFQ", command.RFQId);

        if (rfq.Status != RFQStatus.Closed)
            throw new BusinessRuleException("EvaluateBids",
                $"Bids can only be evaluated for Closed RFQs. Current status: {rfq.Status}");

        var submitted = await _quotationRepo.GetByRFQIdAsync(command.RFQId, ct);
        var submittedMap = submitted
            .Where(q => q.Status == QuotationStatus.Submitted)
            .ToDictionary(q => q.Id);

        if (submittedMap.Count == 0)
            throw new BusinessRuleException("EvaluateBids", "No submitted quotations found for this RFQ.");

        var minPrice = submittedMap.Values.Min(q => q.TotalPrice);

        var evaluation = BidEvaluation.Create(
            command.RFQId, command.PriceWeight, command.QualityWeight, command.DeliveryWeight);

        foreach (var scoreInput in command.Scores)
        {
            if (!submittedMap.TryGetValue(scoreInput.QuotationId, out var quotation))
                continue;

            var priceScore = minPrice > 0
                ? Math.Round(minPrice / quotation.TotalPrice * 100m, 2)
                : 100m;

            var weighted = evaluation.CalculateWeightedScore(priceScore, scoreInput.QualityScore, scoreInput.DeliveryScore);

            evaluation.Scores.Add(new EvaluationScore
            {
                QuotationId   = scoreInput.QuotationId,
                VendorId      = scoreInput.VendorId,
                PriceScore    = priceScore,
                QualityScore  = scoreInput.QualityScore,
                DeliveryScore = scoreInput.DeliveryScore,
                WeightedTotal = weighted,
            });
        }

        var existing = await _evalRepo.GetByRFQIdAsync(command.RFQId, ct);
        if (existing is not null)
        {
            existing.Scores.Clear();
            foreach (var s in evaluation.Scores) existing.Scores.Add(s);
            _evalRepo.Update(existing);
            await _evalRepo.SaveChangesAsync(ct);
            return existing.Id;
        }

        _evalRepo.Add(evaluation);
        await _evalRepo.SaveChangesAsync(ct);
        return evaluation.Id;
    }
}
