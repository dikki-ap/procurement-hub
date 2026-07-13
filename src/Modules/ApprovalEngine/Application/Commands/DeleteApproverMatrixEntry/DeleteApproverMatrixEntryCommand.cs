using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.DeleteApproverMatrixEntry;

public record DeleteApproverMatrixEntryCommand(Guid Id) : ICommand;
