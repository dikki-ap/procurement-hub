using MediatR;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.FinalizeEvaluation;

public class FinalizeEvaluationCommandHandler : ICommandHandler<FinalizeEvaluationCommand>
{
    private readonly IBidEvaluationRepository   _evalRepo;
    private readonly IVendorQuotationRepository _quotationRepo;

    public FinalizeEvaluationCommandHandler(
        IBidEvaluationRepository   evalRepo,
        IVendorQuotationRepository quotationRepo)
    {
        _evalRepo      = evalRepo;
        _quotationRepo = quotationRepo;
    }

    public async Task<Unit> Handle(FinalizeEvaluationCommand cmd, CancellationToken ct)
    {
        var evaluation = await _evalRepo.GetByRFQIdFullAsync(cmd.RFQId, ct)
            ?? throw new NotFoundException("BidEvaluation for RFQ", cmd.RFQId);

        if (evaluation.Status == EvaluationStatus.Awarded)
            throw new BusinessRuleException("FinalizeEvaluation", "Evaluation has already been awarded.");

        if (!evaluation.HasAllEvaluatorsSubmitted())
            throw new BusinessRuleException("FinalizeEvaluation",
                "All assigned evaluators must submit their scores before finalizing.");

        // Gather all submitted quotations and calculate price scores
        var quotations = await _quotationRepo.GetByRFQIdAsync(cmd.RFQId, ct);
        var submitted  = quotations.Where(q => q.Status == QuotationStatus.Submitted).ToList();

        if (submitted.Count == 0)
            throw new BusinessRuleException("FinalizeEvaluation", "No submitted quotations found.");

        var minPrice = submitted.Min(q => q.TotalPrice);

        // Average evaluator scores per quotation
        evaluation.Scores.Clear();

        foreach (var quotation in submitted)
        {
            var evalScores = evaluation.Evaluators
                .SelectMany(a => a.Scores)
                .Where(s => s.QuotationId == quotation.Id)
                .ToList();

            if (evalScores.Count == 0) continue;

            var avgQuality  = evalScores.Average(s => s.QualityScore);
            var avgDelivery = evalScores.Average(s => s.DeliveryScore);
            var priceScore  = minPrice > 0
                ? Math.Round(minPrice / quotation.TotalPrice * 100m, 2)
                : 100m;

            var weighted = evaluation.CalculateWeightedScore(priceScore, (decimal)avgQuality, (decimal)avgDelivery);

            evaluation.Scores.Add(new EvaluationScore
            {
                QuotationId   = quotation.Id,
                VendorId      = quotation.VendorId,
                PriceScore    = priceScore,
                QualityScore  = Math.Round((decimal)avgQuality, 2),
                DeliveryScore = Math.Round((decimal)avgDelivery, 2),
                WeightedTotal = weighted,
            });
        }

        _evalRepo.Update(evaluation);
        await _evalRepo.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
