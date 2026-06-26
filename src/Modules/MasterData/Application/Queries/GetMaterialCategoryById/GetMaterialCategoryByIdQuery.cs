using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetMaterialCategoryById;

public record GetMaterialCategoryByIdQuery(Guid Id) : IQuery<MaterialCategoryDto>;
