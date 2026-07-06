using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class EvaluationScore : BaseAuditableEntity
{
    public Guid    BidEvaluationId { get; set; }
    public Guid    QuotationId     { get; set; }
    public Guid    VendorId        { get; set; }
    public decimal PriceScore      { get; set; }   // 0–100, auto-calculated
    public decimal QualityScore    { get; set; }   // 0–100, entered by purchasing
    public decimal DeliveryScore   { get; set; }   // 0–100, entered by purchasing
    public decimal WeightedTotal   { get; set; }   // calculated by BidEvaluation

    // Navigation
    public BidEvaluation Evaluation { get; set; } = null!;
}
