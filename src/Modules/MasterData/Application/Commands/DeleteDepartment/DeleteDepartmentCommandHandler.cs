using MediatR;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.DeleteDepartment;

public class DeleteDepartmentCommandHandler : ICommandHandler<DeleteDepartmentCommand>
{
    private readonly IDepartmentRepository _repo;

    public DeleteDepartmentCommandHandler(IDepartmentRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteDepartmentCommand command, CancellationToken ct)
    {
        var department = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("Department", command.Id);

        if (await _repo.IsUsedByUsersAsync(command.Id, ct))
            throw new BusinessRuleException("DeleteDepartment",
                "Cannot delete a department that is assigned to one or more users.");

        _repo.Remove(department);
        await _repo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
