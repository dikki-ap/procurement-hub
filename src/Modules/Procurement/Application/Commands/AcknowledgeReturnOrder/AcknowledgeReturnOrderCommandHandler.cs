using MediatR;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.AcknowledgeReturnOrder;

public class AcknowledgeReturnOrderCommandHandler : ICommandHandler<AcknowledgeReturnOrderCommand>
{
    private readonly IReturnOrderRepository _repo;

    public AcknowledgeReturnOrderCommandHandler(IReturnOrderRepository repo) => _repo = repo;

    public async Task<Unit> Handle(AcknowledgeReturnOrderCommand command, CancellationToken ct)
    {
        var returnOrder = await _repo.GetByIdAsync(command.ReturnOrderId, ct)
            ?? throw new NotFoundException("ReturnOrder", command.ReturnOrderId);

        if (returnOrder.VendorId != command.VendorId)
            throw new ForbiddenException("You do not have access to this return order.");

        returnOrder.Acknowledge();
        _repo.Update(returnOrder);
        await _repo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
