using MediatR;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.DeleteApprovalPolicy;

public class DeleteApprovalPolicyCommandHandler : ICommandHandler<DeleteApprovalPolicyCommand>
{
    private readonly IApprovalPolicyRepository _repo;

    public DeleteApprovalPolicyCommandHandler(IApprovalPolicyRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteApprovalPolicyCommand command, CancellationToken ct)
    {
        var policy = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("ApprovalPolicy", command.Id);

        _repo.Remove(policy);
        await _repo.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
