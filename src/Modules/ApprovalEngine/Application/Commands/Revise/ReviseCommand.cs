using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.Revise;

public record ReviseCommand(Guid WorkflowId, Guid ApproverId, string ApproverName, string Reason) : ICommand;
