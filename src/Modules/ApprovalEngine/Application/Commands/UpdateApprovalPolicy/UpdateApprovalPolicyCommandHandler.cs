using MediatR;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.UpdateApprovalPolicy;

public class UpdateApprovalPolicyCommandHandler : ICommandHandler<UpdateApprovalPolicyCommand>
{
    private readonly IApprovalPolicyRepository _repo;

    public UpdateApprovalPolicyCommandHandler(IApprovalPolicyRepository repo) => _repo = repo;

    public async Task<Unit> Handle(UpdateApprovalPolicyCommand command, CancellationToken ct)
    {
        var policy = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("ApprovalPolicy", command.Id);

        policy.Name                   = command.Name;
        policy.ReferenceType          = command.ReferenceType;
        policy.MinValue               = command.MinValue;
        policy.MaxValue               = command.MaxValue;
        policy.RequiredLevels         = command.RequiredLevels;
        policy.IsStrategicOverride    = command.IsStrategicOverride;
        policy.IsSingleSourceOverride = command.IsSingleSourceOverride;
        policy.IsActive               = command.IsActive;

        _repo.Update(policy);
        await _repo.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
