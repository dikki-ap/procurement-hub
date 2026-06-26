using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetUnitOfMeasureById;

public record GetUnitOfMeasureByIdQuery(Guid Id) : IQuery<UnitOfMeasureDto>;
