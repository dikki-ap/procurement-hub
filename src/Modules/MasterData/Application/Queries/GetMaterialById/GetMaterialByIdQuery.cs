using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetMaterialById;

public record GetMaterialByIdQuery(Guid Id) : IQuery<MaterialDto>;
