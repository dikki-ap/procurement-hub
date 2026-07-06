using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetPOList;

public record GetPOListQuery(Guid CompanyId) : IQuery<List<POListDto>>;
