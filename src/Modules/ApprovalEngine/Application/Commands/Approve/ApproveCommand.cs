using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.Approve;

public record ApproveCommand(Guid WorkflowId, Guid ApproverId, string ApproverName) : ICommand;
