using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.CreateContract;

public class CreateContractCommandHandler : ICommandHandler<CreateContractCommand, Guid>
{
    private readonly IContractRepository _repo;

    public CreateContractCommandHandler(IContractRepository repo) => _repo = repo;

    public async Task<Guid> Handle(CreateContractCommand cmd, CancellationToken ct)
    {
        var number = await _repo.GenerateNextNumberAsync(cmd.CompanyId, ct);

        var contract = new Contract
        {
            CompanyId      = cmd.CompanyId,
            VendorId       = cmd.VendorId,
            Title          = cmd.Title,
            ContractNumber = number,
            POId           = cmd.POId,
            StartDate      = cmd.StartDate,
            EndDate        = cmd.EndDate,
            Value          = cmd.Value,
            CurrencyId     = cmd.CurrencyId,
            Notes          = cmd.Notes,
        };

        _repo.Add(contract);
        await _repo.SaveChangesAsync(ct);
        return contract.Id;
    }
}
