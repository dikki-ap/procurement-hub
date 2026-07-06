using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetRFQList;

public record GetRFQListQuery(Guid CompanyId) : IQuery<List<RFQListDto>>;
