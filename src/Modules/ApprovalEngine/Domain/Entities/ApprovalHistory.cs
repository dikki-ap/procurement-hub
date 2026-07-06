using ProcureHub.Modules.ApprovalEngine.Domain.Enums;
using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.ApprovalEngine.Domain.Entities;

public class ApprovalHistory : BaseAuditableEntity
{
    public Guid               WorkflowId  { get; set; }
    public int                Level       { get; set; }
    public ApprovalActionType Action      { get; set; }
    public Guid               ActorId     { get; set; }
    public string             ActorName   { get; set; } = string.Empty;
    public string?            Reason      { get; set; }
    public DateTime           ActedAt     { get; set; } = DateTime.UtcNow;

    // Navigation
    public ApprovalWorkflow? Workflow { get; set; }
}
