using MediatR;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.TerminateContract;

public class TerminateContractCommandHandler : ICommandHandler<TerminateContractCommand>
{
    private readonly IContractRepository _repo;
    private readonly IPublisher          _publisher;

    public TerminateContractCommandHandler(IContractRepository repo, IPublisher publisher)
    {
        _repo      = repo;
        _publisher = publisher;
    }

    public async Task<Unit> Handle(TerminateContractCommand cmd, CancellationToken ct)
    {
        var contract = await _repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException("Contract", cmd.Id);

        contract.Terminate(cmd.Reason);

        _repo.Update(contract);
        await _repo.SaveChangesAsync(ct);

        foreach (var evt in contract.DomainEvents)
            await _publisher.Publish(evt, ct);

        contract.ClearDomainEvents();
        return Unit.Value;
    }
}
