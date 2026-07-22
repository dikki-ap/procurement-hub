using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetContractById;

public record GetContractByIdQuery(Guid Id) : IQuery<ContractDto>;
