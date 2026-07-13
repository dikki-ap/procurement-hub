using ProcureHub.Modules.ApprovalEngine.Domain.Entities;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.CreateApproverMatrixEntry;

public class CreateApproverMatrixEntryCommandHandler : ICommandHandler<CreateApproverMatrixEntryCommand, Guid>
{
    private readonly IApproverMatrixRepository _repo;

    public CreateApproverMatrixEntryCommandHandler(IApproverMatrixRepository repo) => _repo = repo;

    public async Task<Guid> Handle(CreateApproverMatrixEntryCommand command, CancellationToken ct)
    {
        var exists = await _repo.ExistsAsync(
            command.CompanyId, command.ReferenceType, command.Level, command.Email, null, ct);

        if (exists)
            throw new ConflictException(
                $"An approver matrix entry for {command.ReferenceType} Level {command.Level} with email '{command.Email}' already exists.");

        var entry = new ApproverMatrixEntry
        {
            CompanyId     = command.CompanyId,
            ReferenceType = command.ReferenceType,
            Level         = command.Level,
            Name          = command.Name,
            Position      = command.Position,
            Email         = command.Email,
        };

        _repo.Add(entry);
        await _repo.SaveChangesAsync(ct);
        return entry.Id;
    }
}
