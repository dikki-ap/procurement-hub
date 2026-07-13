using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.UpdateApproverMatrixEntry;

public record UpdateApproverMatrixEntryCommand(
    Guid   Id,
    string ReferenceType,
    int    Level,
    string Name,
    string Position,
    string Email) : ICommand;
