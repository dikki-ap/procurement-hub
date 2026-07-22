using MediatR;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.ConfirmReturnReceived;

public class ConfirmReturnReceivedCommandHandler : ICommandHandler<ConfirmReturnReceivedCommand>
{
    private readonly IReturnOrderRepository _repo;

    public ConfirmReturnReceivedCommandHandler(IReturnOrderRepository repo) => _repo = repo;

    public async Task<Unit> Handle(ConfirmReturnReceivedCommand command, CancellationToken ct)
    {
        var returnOrder = await _repo.GetByIdAsync(command.ReturnOrderId, ct)
            ?? throw new NotFoundException("ReturnOrder", command.ReturnOrderId);

        returnOrder.ConfirmReceived();
        _repo.Update(returnOrder);
        await _repo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
