using MediatR;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.ActivateContract;

public class ActivateContractCommandHandler : ICommandHandler<ActivateContractCommand>
{
    private readonly IContractRepository _repo;
    private readonly IPublisher          _publisher;

    public ActivateContractCommandHandler(IContractRepository repo, IPublisher publisher)
    {
        _repo      = repo;
        _publisher = publisher;
    }

    public async Task<Unit> Handle(ActivateContractCommand cmd, CancellationToken ct)
    {
        var contract = await _repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException("Contract", cmd.Id);

        contract.Activate();

        _repo.Update(contract);
        await _repo.SaveChangesAsync(ct);

        foreach (var evt in contract.DomainEvents)
            await _publisher.Publish(evt, ct);

        contract.ClearDomainEvents();
        return Unit.Value;
    }
}
