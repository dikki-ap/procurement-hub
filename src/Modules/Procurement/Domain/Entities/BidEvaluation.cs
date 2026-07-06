using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Events;
using ProcureHub.SharedKernel.Domain;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class BidEvaluation : AggregateRoot
{
    public Guid             RFQId              { get; set; }
    public decimal          PriceWeight        { get; set; }  // percentage, e.g. 33.33
    public decimal          QualityWeight      { get; set; }
    public decimal          DeliveryWeight     { get; set; }
    public EvaluationStatus Status             { get; set; } = EvaluationStatus.Pending;
    public Guid?            AwardedVendorId    { get; set; }
    public Guid?            AwardedQuotationId { get; set; }

    // Navigation
    public ICollection<EvaluationScore> Scores { get; set; } = [];

    // ── Factory ──────────────────────────────────────────────────────────────

    public static BidEvaluation Create(Guid rfqId, decimal priceWeight, decimal qualityWeight, decimal deliveryWeight)
    {
        var sum = priceWeight + qualityWeight + deliveryWeight;
        if (Math.Abs(sum - 100m) > 0.01m)
            throw new BusinessRuleException("BidEvaluation",
                $"Weights must sum to 100. Got: {sum}");

        return new BidEvaluation
        {
            RFQId          = rfqId,
            PriceWeight    = priceWeight,
            QualityWeight  = qualityWeight,
            DeliveryWeight = deliveryWeight,
        };
    }

    // ── Domain methods ──────────────────────────────────────────────────────

    public decimal CalculateWeightedScore(decimal priceScore, decimal qualityScore, decimal deliveryScore)
        => Math.Round(
            (priceScore * PriceWeight + qualityScore * QualityWeight + deliveryScore * DeliveryWeight) / 100m, 2);

    public void Award(Guid quotationId, Guid vendorId)
    {
        if (Status == EvaluationStatus.Awarded)
            throw new BusinessRuleException("BidAward", "Bid has already been awarded.");

        AwardedQuotationId = quotationId;
        AwardedVendorId    = vendorId;
        Status             = EvaluationStatus.Awarded;

        AddDomainEvent(new BidAwardedEvent(Id, RFQId, quotationId, vendorId));
    }
}
