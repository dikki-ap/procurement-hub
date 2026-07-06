using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.ApprovalEngine.Domain.Entities;
using ProcureHub.Modules.ApprovalEngine.Domain.Enums;
using ProcureHub.SharedKernel.Database;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.ApprovalEngine.Domain.Services;

public class ApprovalStateMachine
{
    private readonly ApplicationDbContext _context;

    public ApprovalStateMachine(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> DetermineRequiredLevels(
        string referenceType,
        decimal totalValue,
        bool isStrategicItem,
        bool isSingleSource,
        Guid companyId,
        CancellationToken ct = default)
    {
        var policies = await _context.Set<ApprovalPolicy>()
            .Where(p => p.CompanyId == companyId
                     && p.ReferenceType == referenceType
                     && p.IsActive)
            .OrderBy(p => p.MinValue)
            .ToListAsync(ct);

        if (!policies.Any()) return 1;

        var matchingPolicy = policies.FirstOrDefault(p =>
            totalValue >= p.MinValue &&
            (p.MaxValue == null || totalValue < p.MaxValue))
            ?? policies.Last();

        var levels            = matchingPolicy.RequiredLevels;
        var maxPossibleLevels = policies.Max(p => p.RequiredLevels);

        if (isStrategicItem && matchingPolicy.IsStrategicOverride)
            levels = Math.Min(levels + 1, maxPossibleLevels);

        if (isSingleSource && matchingPolicy.IsSingleSourceOverride)
            levels = Math.Min(levels + 1, maxPossibleLevels);

        return levels;
    }

    public void Approve(ApprovalWorkflow workflow, Guid approverId, string actorName)
    {
        ValidatePendingState(workflow);
        ValidateCurrentApprover(workflow, approverId);

        workflow.AddHistory(workflow.CurrentLevel, ApprovalActionType.Approve, approverId, actorName);

        if (workflow.CurrentLevel < workflow.MaxLevel)
            workflow.CurrentLevel++;
        else
            workflow.MarkApproved();
    }

    public void Revise(ApprovalWorkflow workflow, Guid approverId, string actorName, string reason)
    {
        ValidatePendingState(workflow);
        ValidateCurrentApprover(workflow, approverId);
        ValidateReason(reason);

        workflow.AddHistory(workflow.CurrentLevel, ApprovalActionType.Revise, approverId, actorName, reason);

        if (workflow.CurrentLevel > 1)
            workflow.CurrentLevel--;
        else
            workflow.MarkRevised(reason);
    }

    public void Reject(ApprovalWorkflow workflow, Guid approverId, string actorName, string reason)
    {
        ValidatePendingState(workflow);
        ValidateCurrentApprover(workflow, approverId);
        ValidateReason(reason);

        workflow.AddHistory(workflow.CurrentLevel, ApprovalActionType.Reject, approverId, actorName, reason);
        workflow.MarkRejected(reason);
    }

    public void Delegate(
        ApprovalWorkflow workflow,
        Guid approverId,
        string actorName,
        Guid delegateToUserId,
        string delegateToUserName)
    {
        ValidatePendingState(workflow);
        ValidateCurrentApprover(workflow, approverId);

        if (approverId == delegateToUserId)
            throw new BusinessRuleException("DelegateApproval", "Cannot delegate to yourself.");

        var currentAssignment = workflow.Assignments
            .FirstOrDefault(a => a.Level == workflow.CurrentLevel && !a.IsDelegate);

        workflow.Assignments.Add(new ApproverAssignment
        {
            WorkflowId       = workflow.Id,
            Level            = workflow.CurrentLevel,
            AssignedUserId   = delegateToUserId,
            AssignedUserName = delegateToUserName,
            IsDelegate       = true,
            DelegatedFromId  = currentAssignment?.Id,
        });

        workflow.AddHistory(workflow.CurrentLevel, ApprovalActionType.Delegate,
            approverId, actorName, $"Delegated to {delegateToUserName}");
    }

    private static void ValidatePendingState(ApprovalWorkflow workflow)
    {
        if (workflow.Status != WorkflowStatus.Pending)
            throw new BusinessRuleException("ApprovalAction", "Workflow is not in pending state.");
    }

    private static void ValidateCurrentApprover(ApprovalWorkflow workflow, Guid approverId)
    {
        var assignments = workflow.Assignments.Where(a => a.Level == workflow.CurrentLevel).ToList();
        if (assignments.Count > 0 && assignments.All(a => a.AssignedUserId != approverId))
            throw new ForbiddenException("You are not assigned to approve this workflow at the current level.");
    }

    private static void ValidateReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 20)
            throw new BusinessRuleException("ApprovalReason", "Reason must be at least 20 characters.");
    }
}
