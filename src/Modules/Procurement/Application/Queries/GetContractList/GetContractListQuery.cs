using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetContractList;

public record GetContractListQuery(Guid CompanyId) : IQuery<List<ContractListDto>>;
