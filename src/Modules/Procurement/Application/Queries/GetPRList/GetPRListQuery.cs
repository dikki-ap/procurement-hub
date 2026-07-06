using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetPRList;

public record GetPRListQuery(Guid CompanyId) : IQuery<List<PRListDto>>;
