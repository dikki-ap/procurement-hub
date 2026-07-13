using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.DeleteApprovalPolicy;

public record DeleteApprovalPolicyCommand(Guid Id) : ICommand;
