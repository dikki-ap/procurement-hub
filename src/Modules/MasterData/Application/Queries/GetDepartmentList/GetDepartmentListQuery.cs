using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetDepartmentList;

public record GetDepartmentListQuery(Guid CompanyId) : IQuery<List<DepartmentDto>>;
