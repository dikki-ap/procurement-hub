using MediatR;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.Modules.ApprovalEngine.Domain.Services;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.Revise;

public class ReviseCommandHandler : ICommandHandler<ReviseCommand>
{
    private readonly IApprovalWorkflowRepository _workflowRepo;
    private readonly ApprovalStateMachine        _stateMachine;

    public ReviseCommandHandler(
        IApprovalWorkflowRepository workflowRepo,
        ApprovalStateMachine stateMachine)
    {
        _workflowRepo = workflowRepo;
        _stateMachine = stateMachine;
    }

    public async Task<Unit> Handle(ReviseCommand command, CancellationToken ct)
    {
        var workflow = await _workflowRepo.GetByIdWithDetailsAsync(command.WorkflowId, ct)
                       ?? throw new NotFoundException("ApprovalWorkflow", command.WorkflowId);

        _stateMachine.Revise(workflow, command.ApproverId, command.ApproverName, command.Reason);

        _workflowRepo.Update(workflow);
        await _workflowRepo.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
