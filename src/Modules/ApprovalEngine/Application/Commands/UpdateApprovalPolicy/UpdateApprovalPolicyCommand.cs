using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.UpdateApprovalPolicy;

public record UpdateApprovalPolicyCommand(
    Guid     Id,
    string   Name,
    string   ReferenceType,
    decimal  MinValue,
    decimal? MaxValue,
    int      RequiredLevels,
    bool     IsStrategicOverride,
    bool     IsSingleSourceOverride,
    bool     IsActive) : ICommand;
