using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.Reject;

public record RejectCommand(Guid WorkflowId, Guid ApproverId, string ApproverName, string Reason) : ICommand;
