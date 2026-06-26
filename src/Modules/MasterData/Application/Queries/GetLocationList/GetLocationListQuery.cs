using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetLocationList;

public record GetLocationListQuery(Guid CompanyId) : IQuery<List<LocationDto>>;
