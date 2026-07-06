using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.ApprovalEngine.Domain.Entities;

public class ApproverAssignment : BaseAuditableEntity
{
    public Guid    WorkflowId         { get; set; }
    public int     Level              { get; set; }
    public Guid    AssignedUserId     { get; set; }
    public string  AssignedUserName   { get; set; } = string.Empty;
    public bool    IsDelegate         { get; set; }
    public Guid?   DelegatedFromId    { get; set; }

    // Navigation
    public ApprovalWorkflow? Workflow { get; set; }
}
