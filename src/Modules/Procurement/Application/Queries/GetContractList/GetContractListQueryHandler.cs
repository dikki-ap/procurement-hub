using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetContractList;

public class GetContractListQueryHandler : IQueryHandler<GetContractListQuery, List<ContractListDto>>
{
    private readonly IContractRepository     _contractRepo;
    private readonly IVendorRepository       _vendorRepo;
    private readonly IPurchaseOrderRepository _poRepo;

    public GetContractListQueryHandler(
        IContractRepository      contractRepo,
        IVendorRepository        vendorRepo,
        IPurchaseOrderRepository poRepo)
    {
        _contractRepo = contractRepo;
        _vendorRepo   = vendorRepo;
        _poRepo       = poRepo;
    }

    public async Task<List<ContractListDto>> Handle(GetContractListQuery query, CancellationToken ct)
    {
        var contracts = await _contractRepo.GetAllAsync(query.CompanyId, ct);
        if (contracts.Count == 0) return [];

        var vendorIds = contracts.Select(c => c.VendorId).Distinct().ToList();
        var vendors   = await _vendorRepo.GetByIdsAsync(vendorIds, ct);
        var vendorMap = vendors.ToDictionary(v => v.Id, v => v.LegalName);

        var poIds = contracts.Where(c => c.POId.HasValue).Select(c => c.POId!.Value).Distinct().ToList();
        var poMap = new Dictionary<Guid, string>();
        if (poIds.Count > 0)
        {
            var pos = await _poRepo.GetByIdsAsync(poIds, ct);
            poMap = pos.ToDictionary(p => p.Id, p => p.PONumber);
        }

        return contracts.Select(c => new ContractListDto(
            c.Id,
            c.ContractNumber,
            c.Title,
            c.VendorId,
            vendorMap.GetValueOrDefault(c.VendorId, c.VendorId.ToString()),
            c.POId,
            c.POId.HasValue ? poMap.GetValueOrDefault(c.POId.Value) : null,
            c.Status,
            c.StartDate,
            c.EndDate,
            c.Value,
            c.CurrencyId,
            c.CreatedAt)).ToList();
    }
}
