using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetGRNList;

public record GetGRNListQuery(Guid POId) : IQuery<List<GRNListDto>>;
