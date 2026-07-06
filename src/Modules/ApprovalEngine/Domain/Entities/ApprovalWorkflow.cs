using ProcureHub.Modules.ApprovalEngine.Domain.Enums;
using ProcureHub.Modules.ApprovalEngine.Domain.Events;
using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.Modules.ApprovalEngine.Domain.Entities;

public class ApprovalWorkflow : AggregateRoot
{
    public Guid           CompanyId        { get; set; }
    public string         ReferenceType    { get; set; } = string.Empty; // "PR", "PO"
    public Guid           ReferenceId      { get; set; }
    public string         ReferenceNumber  { get; set; } = string.Empty;
    public string         ReferenceTitle   { get; set; } = string.Empty;
    public decimal        TotalValue       { get; set; }
    public bool           IsStrategicItem  { get; set; }
    public bool           IsSingleSource   { get; set; }
    public Guid           RequestedById    { get; set; }
    public int            CurrentLevel     { get; set; } = 1;
    public int            MaxLevel         { get; set; } = 1;
    public WorkflowStatus Status           { get; set; } = WorkflowStatus.Pending;
    public int            Iteration        { get; set; } = 1;
    public DateTime?      CompletedAt      { get; set; }

    // Navigation
    public ICollection<ApprovalHistory>      History     { get; set; } = [];
    public ICollection<ApproverAssignment>   Assignments { get; set; } = [];

    public static ApprovalWorkflow Create(
        Guid companyId,
        string referenceType,
        Guid referenceId,
        string referenceNumber,
        string referenceTitle,
        decimal totalValue,
        bool isStrategicItem,
        bool isSingleSource,
        Guid requestedById,
        int maxLevel)
    {
        var workflow = new ApprovalWorkflow
        {
            CompanyId       = companyId,
            ReferenceType   = referenceType,
            ReferenceId     = referenceId,
            ReferenceNumber = referenceNumber,
            ReferenceTitle  = referenceTitle,
            TotalValue      = totalValue,
            IsStrategicItem = isStrategicItem,
            IsSingleSource  = isSingleSource,
            RequestedById   = requestedById,
            MaxLevel        = maxLevel,
        };

        workflow.AddDomainEvent(new ApprovalRequestedEvent(
            workflow.Id, referenceId, referenceType, referenceNumber, maxLevel));

        return workflow;
    }

    public void AddHistory(int level, ApprovalActionType action, Guid actorId, string actorName, string? reason = null)
    {
        History.Add(new ApprovalHistory
        {
            WorkflowId = Id,
            Level      = level,
            Action     = action,
            ActorId    = actorId,
            ActorName  = actorName,
            Reason     = reason,
            ActedAt    = DateTime.UtcNow,
        });
    }

    public void MarkApproved()
    {
        Status      = WorkflowStatus.Approved;
        CompletedAt = DateTime.UtcNow;
        AddDomainEvent(new ApprovalCompletedEvent(
            Id, ReferenceId, ReferenceType, ReferenceNumber, RequestedById));
    }

    public void MarkRevised(string reason)
    {
        Status      = WorkflowStatus.Revised;
        CompletedAt = null;
        AddDomainEvent(new ApprovalRevisedEvent(
            Id, ReferenceId, ReferenceType, ReferenceNumber, RequestedById, reason));
    }

    public void MarkRejected(string reason)
    {
        Status      = WorkflowStatus.Rejected;
        CompletedAt = DateTime.UtcNow;
        Iteration++;
        AddDomainEvent(new ApprovalRejectedEvent(
            Id, ReferenceId, ReferenceType, ReferenceNumber, RequestedById, reason));
    }

    public void Cancel()
    {
        if (Status != WorkflowStatus.Pending)
            return;
        Status = WorkflowStatus.Cancelled;
    }
}
