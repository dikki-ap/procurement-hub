using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetUnitOfMeasureList;

public record GetUnitOfMeasureListQuery(Guid CompanyId) : IQuery<List<UnitOfMeasureDto>>;
