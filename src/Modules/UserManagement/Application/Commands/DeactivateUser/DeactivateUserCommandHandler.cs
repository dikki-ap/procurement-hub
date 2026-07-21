using MediatR;
using ProcureHub.Modules.UserManagement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.UserManagement.Application.Commands.DeactivateUser;

public class DeactivateUserCommandHandler : ICommandHandler<DeactivateUserCommand>
{
    private readonly IUserRepository _repo;

    public DeactivateUserCommandHandler(IUserRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeactivateUserCommand command, CancellationToken ct)
    {
        var user = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("User", command.Id);

        if (!user.IsActive)
            throw new BusinessRuleException("DeactivateUser", "User is already inactive.");

        user.IsActive = false;

        _repo.Update(user);
        await _repo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
