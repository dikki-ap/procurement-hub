using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.DelegateApproval;

public record DelegateApprovalCommand(
    Guid   WorkflowId,
    Guid   ApproverId,
    string ApproverName,
    Guid   DelegateToUserId,
    string DelegateToUserName) : ICommand;
