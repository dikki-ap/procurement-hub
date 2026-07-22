using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class EvaluatorScore : BaseEntity
{
    public Guid    EvaluatorAssignmentId { get; set; }
    public Guid    QuotationId           { get; set; }
    public decimal QualityScore          { get; set; }  // 0–100
    public decimal DeliveryScore         { get; set; }  // 0–100

    // Navigation
    public EvaluatorAssignment Assignment { get; set; } = null!;
}
