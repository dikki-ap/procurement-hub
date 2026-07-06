using ProcureHub.Modules.ApprovalEngine.Domain.Entities;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.Modules.ApprovalEngine.Domain.Services;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.SubmitForApproval;

public class SubmitForApprovalCommandHandler : ICommandHandler<SubmitForApprovalCommand, Guid>
{
    private readonly IApprovalWorkflowRepository _workflowRepo;
    private readonly ApprovalStateMachine        _stateMachine;

    public SubmitForApprovalCommandHandler(
        IApprovalWorkflowRepository workflowRepo,
        ApprovalStateMachine stateMachine)
    {
        _workflowRepo = workflowRepo;
        _stateMachine = stateMachine;
    }

    public async Task<Guid> Handle(SubmitForApprovalCommand command, CancellationToken ct)
    {
        var existing = await _workflowRepo.GetByReferenceAsync(
            command.ReferenceType, command.ReferenceId, ct);

        if (existing is not null && existing.Status == Domain.Enums.WorkflowStatus.Pending)
            throw new ConflictException("An active approval workflow already exists for this document.");

        var maxLevel = await _stateMachine.DetermineRequiredLevels(
            command.ReferenceType, command.TotalValue,
            command.IsStrategicItem, command.IsSingleSource,
            command.CompanyId, ct);

        var workflow = ApprovalWorkflow.Create(
            command.CompanyId,
            command.ReferenceType,
            command.ReferenceId,
            command.ReferenceNumber,
            command.ReferenceTitle,
            command.TotalValue,
            command.IsStrategicItem,
            command.IsSingleSource,
            command.RequestedById,
            maxLevel);

        foreach (var approver in command.Approvers)
        {
            workflow.Assignments.Add(new ApproverAssignment
            {
                WorkflowId       = workflow.Id,
                Level            = approver.Level,
                AssignedUserId   = approver.UserId,
                AssignedUserName = approver.UserName,
                IsDelegate       = false,
            });
        }

        _workflowRepo.Add(workflow);
        await _workflowRepo.SaveChangesAsync(ct);
        return workflow.Id;
    }
}
