using MediatR;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.DeleteApproverMatrixEntry;

public class DeleteApproverMatrixEntryCommandHandler : ICommandHandler<DeleteApproverMatrixEntryCommand>
{
    private readonly IApproverMatrixRepository _repo;

    public DeleteApproverMatrixEntryCommandHandler(IApproverMatrixRepository repo) => _repo = repo;

    public async Task<Unit> Handle(DeleteApproverMatrixEntryCommand command, CancellationToken ct)
    {
        var entry = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new NotFoundException("ApproverMatrixEntry", command.Id);

        _repo.Remove(entry);
        await _repo.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
