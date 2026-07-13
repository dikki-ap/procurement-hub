using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.CreateApproverMatrixEntry;

public record CreateApproverMatrixEntryCommand(
    Guid   CompanyId,
    string ReferenceType,
    int    Level,
    string Name,
    string Position,
    string Email) : ICommand<Guid>;
