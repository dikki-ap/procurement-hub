using ProcureHub.Modules.ApprovalEngine.Domain.Entities;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.CreateApprovalPolicy;

public class CreateApprovalPolicyCommandHandler : ICommandHandler<CreateApprovalPolicyCommand, Guid>
{
    private readonly IApprovalPolicyRepository _policyRepo;

    public CreateApprovalPolicyCommandHandler(IApprovalPolicyRepository policyRepo)
    {
        _policyRepo = policyRepo;
    }

    public async Task<Guid> Handle(CreateApprovalPolicyCommand command, CancellationToken ct)
    {
        var exists = await _policyRepo.ExistsAsync(
            command.CompanyId, command.ReferenceType, command.MinValue, ct);

        if (exists)
            throw new ConflictException(
                $"A policy for {command.ReferenceType} with MinValue {command.MinValue} already exists.");

        var policy = new ApprovalPolicy
        {
            CompanyId              = command.CompanyId,
            ReferenceType          = command.ReferenceType,
            Name                   = command.Name,
            MinValue               = command.MinValue,
            MaxValue               = command.MaxValue,
            RequiredLevels         = command.RequiredLevels,
            IsStrategicOverride    = command.IsStrategicOverride,
            IsSingleSourceOverride = command.IsSingleSourceOverride,
            IsActive               = true,
        };

        _policyRepo.Add(policy);
        await _policyRepo.SaveChangesAsync(ct);
        return policy.Id;
    }
}
