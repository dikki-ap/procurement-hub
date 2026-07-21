using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Domain;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateDepartment;

public class CreateDepartmentCommandHandler : ICommandHandler<CreateDepartmentCommand, Guid>
{
    private readonly IDepartmentRepository _repo;

    public CreateDepartmentCommandHandler(IDepartmentRepository repo) => _repo = repo;

    public async Task<Guid> Handle(CreateDepartmentCommand command, CancellationToken ct)
    {
        if (await _repo.ExistsByCodeAsync(command.CompanyId, command.Code.ToUpperInvariant(), null, ct))
            throw new ConflictException("Department", "Code", command.Code);

        var department = new Department
        {
            CompanyId = command.CompanyId,
            Name      = command.Name,
            Code      = command.Code.ToUpperInvariant(),
            IsActive  = command.IsActive,
        };

        _repo.Add(department);
        await _repo.SaveChangesAsync(ct);

        return department.Id;
    }
}
