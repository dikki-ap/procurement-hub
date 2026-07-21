using MediatR;
using ProcureHub.Modules.UserManagement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.UserManagement.Application.Commands.UpdateUser;

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand>
{
    private static readonly HashSet<string> ValidRoles =
    [
        "super_admin", "requester", "purchasing", "approver", "finance", "management"
    ];

    private readonly IUserRepository _repo;

    public UpdateUserCommandHandler(IUserRepository repo) => _repo = repo;

    public async Task<Unit> Handle(UpdateUserCommand command, CancellationToken ct)
    {
        if (!ValidRoles.Contains(command.Role))
            throw new BusinessRuleException("UpdateUser", $"'{command.Role}' is not a valid internal role.");

        var user = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("User", command.Id);

        user.DepartmentId = command.DepartmentId;
        user.Role         = command.Role;
        user.IsActive     = command.IsActive;

        _repo.Update(user);
        await _repo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
