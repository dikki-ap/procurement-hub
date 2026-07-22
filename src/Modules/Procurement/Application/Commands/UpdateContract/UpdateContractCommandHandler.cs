using MediatR;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.UpdateContract;

public class UpdateContractCommandHandler : ICommandHandler<UpdateContractCommand>
{
    private readonly IContractRepository _repo;

    public UpdateContractCommandHandler(IContractRepository repo) => _repo = repo;

    public async Task<Unit> Handle(UpdateContractCommand cmd, CancellationToken ct)
    {
        var contract = await _repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException("Contract", cmd.Id);

        if (contract.Status != ContractStatus.Draft)
            throw new BusinessRuleException("ContractUpdate",
                "Only draft contracts can be edited.");

        contract.Title      = cmd.Title;
        contract.POId       = cmd.POId;
        contract.StartDate  = cmd.StartDate;
        contract.EndDate    = cmd.EndDate;
        contract.Value      = cmd.Value;
        contract.CurrencyId = cmd.CurrencyId;
        contract.Notes      = cmd.Notes;

        _repo.Update(contract);
        await _repo.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
