using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.SubmitForApproval;

public record SubmitForApprovalCommand(
    Guid    CompanyId,
    string  ReferenceType,
    Guid    ReferenceId,
    string  ReferenceNumber,
    string  ReferenceTitle,
    decimal TotalValue,
    bool    IsStrategicItem,
    bool    IsSingleSource,
    Guid    RequestedById,
    List<ApproverLevelRequest> Approvers) : ICommand<Guid>;

public record ApproverLevelRequest(int Level, Guid UserId, string UserName);
