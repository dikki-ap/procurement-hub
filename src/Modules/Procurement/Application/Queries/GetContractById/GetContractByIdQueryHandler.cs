using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetContractById;

public class GetContractByIdQueryHandler : IQueryHandler<GetContractByIdQuery, ContractDto>
{
    private readonly IContractRepository      _contractRepo;
    private readonly IVendorRepository        _vendorRepo;
    private readonly IPurchaseOrderRepository _poRepo;

    public GetContractByIdQueryHandler(
        IContractRepository      contractRepo,
        IVendorRepository        vendorRepo,
        IPurchaseOrderRepository poRepo)
    {
        _contractRepo = contractRepo;
        _vendorRepo   = vendorRepo;
        _poRepo       = poRepo;
    }

    public async Task<ContractDto> Handle(GetContractByIdQuery query, CancellationToken ct)
    {
        var c = await _contractRepo.GetByIdAsync(query.Id, ct)
            ?? throw new NotFoundException("Contract", query.Id);

        var vendor = await _vendorRepo.GetByIdAsync(c.VendorId, ct);
        var po     = c.POId.HasValue ? await _poRepo.GetByIdAsync(c.POId.Value, ct) : null;

        return new ContractDto(
            c.Id,
            c.CompanyId,
            c.ContractNumber,
            c.Title,
            c.VendorId,
            vendor?.LegalName ?? c.VendorId.ToString(),
            c.POId,
            po?.PONumber,
            c.Status,
            !string.IsNullOrEmpty(c.FileKey),
            c.SignedAt,
            c.StartDate,
            c.EndDate,
            c.Value,
            c.CurrencyId,
            c.Notes,
            c.CreatedAt,
            c.UpdatedAt,
            c.CreatedBy?.FullName,
            c.UpdatedBy?.FullName);
    }
}
