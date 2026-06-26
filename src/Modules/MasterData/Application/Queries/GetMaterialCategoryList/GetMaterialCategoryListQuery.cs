using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetMaterialCategoryList;

public record GetMaterialCategoryListQuery(Guid CompanyId) : IQuery<List<MaterialCategoryDto>>;
