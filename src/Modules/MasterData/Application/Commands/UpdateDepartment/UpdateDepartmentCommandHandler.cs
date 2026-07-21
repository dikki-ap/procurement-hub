using MediatR;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateDepartment;

public class UpdateDepartmentCommandHandler : ICommandHandler<UpdateDepartmentCommand>
{
    private readonly IDepartmentRepository _repo;

    public UpdateDepartmentCommandHandler(IDepartmentRepository repo) => _repo = repo;

    public async Task<Unit> Handle(UpdateDepartmentCommand command, CancellationToken ct)
    {
        var department = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("Department", command.Id);

        if (await _repo.ExistsByCodeAsync(department.CompanyId, command.Code.ToUpperInvariant(), command.Id, ct))
            throw new ConflictException("Department", "Code", command.Code);

        department.Name     = command.Name;
        department.Code     = command.Code.ToUpperInvariant();
        department.IsActive = command.IsActive;

        _repo.Update(department);
        await _repo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
