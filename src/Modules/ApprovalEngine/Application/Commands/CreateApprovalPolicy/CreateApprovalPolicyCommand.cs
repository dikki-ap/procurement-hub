using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.CreateApprovalPolicy;

public record CreateApprovalPolicyCommand(
    Guid    CompanyId,
    string  ReferenceType,
    string  Name,
    decimal MinValue,
    decimal? MaxValue,
    int     RequiredLevels,
    bool    IsStrategicOverride,
    bool    IsSingleSourceOverride) : ICommand<Guid>;
