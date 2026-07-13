using MediatR;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.UpdateApproverMatrixEntry;

public class UpdateApproverMatrixEntryCommandHandler : ICommandHandler<UpdateApproverMatrixEntryCommand>
{
    private readonly IApproverMatrixRepository _repo;

    public UpdateApproverMatrixEntryCommandHandler(IApproverMatrixRepository repo) => _repo = repo;

    public async Task<Unit> Handle(UpdateApproverMatrixEntryCommand command, CancellationToken ct)
    {
        var entry = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("ApproverMatrixEntry", command.Id);

        var exists = await _repo.ExistsAsync(
            entry.CompanyId, command.ReferenceType, command.Level, command.Email, command.Id, ct);

        if (exists)
            throw new ConflictException(
                $"An approver matrix entry for {command.ReferenceType} Level {command.Level} with email '{command.Email}' already exists.");

        entry.ReferenceType = command.ReferenceType;
        entry.Level         = command.Level;
        entry.Name          = command.Name;
        entry.Position      = command.Position;
        entry.Email         = command.Email;

        _repo.Update(entry);
        await _repo.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
