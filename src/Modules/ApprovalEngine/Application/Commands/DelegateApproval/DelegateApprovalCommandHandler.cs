using MediatR;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.Modules.ApprovalEngine.Domain.Services;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.DelegateApproval;

public class DelegateApprovalCommandHandler : ICommandHandler<DelegateApprovalCommand>
{
    private readonly IApprovalWorkflowRepository _workflowRepo;
    private readonly ApprovalStateMachine        _stateMachine;

    public DelegateApprovalCommandHandler(
        IApprovalWorkflowRepository workflowRepo,
        ApprovalStateMachine stateMachine)
    {
        _workflowRepo = workflowRepo;
        _stateMachine = stateMachine;
    }

    public async Task<Unit> Handle(DelegateApprovalCommand command, CancellationToken ct)
    {
        var workflow = await _workflowRepo.GetByIdWithDetailsAsync(command.WorkflowId, ct)
                       ?? throw new NotFoundException("ApprovalWorkflow", command.WorkflowId);

        _stateMachine.Delegate(workflow, command.ApproverId, command.ApproverName,
            command.DelegateToUserId, command.DelegateToUserName);

        _workflowRepo.Update(workflow);
        await _workflowRepo.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
