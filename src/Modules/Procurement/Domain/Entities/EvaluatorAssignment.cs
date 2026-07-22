using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.Procurement.Domain.Entities;

public class EvaluatorAssignment : BaseAuditableEntity
{
    public Guid   BidEvaluationId    { get; set; }
    public Guid   AssignedUserId     { get; set; }
    public string AssignedUserName   { get; set; } = string.Empty;
    public bool   HasSubmitted       { get; set; } = false;

    // Navigation
    public BidEvaluation                 Evaluation { get; set; } = null!;
    public ICollection<EvaluatorScore>   Scores     { get; set; } = [];
}
