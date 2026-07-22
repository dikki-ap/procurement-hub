using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetContractsByVendor;

public record GetContractsByVendorQuery(Guid VendorId) : IQuery<List<ContractListDto>>;
