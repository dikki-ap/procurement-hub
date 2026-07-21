using ProcureHub.Modules.MasterData.Application.DTOs;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.MasterData.Application.Queries.GetDepartmentList;

public class GetDepartmentListQueryHandler
    : IQueryHandler<GetDepartmentListQuery, List<DepartmentDto>>
{
    private readonly IDepartmentRepository _repo;

    public GetDepartmentListQueryHandler(IDepartmentRepository repo) => _repo = repo;

    public async Task<List<DepartmentDto>> Handle(
        GetDepartmentListQuery request,
        CancellationToken cancellationToken)
    {
        var departments = await _repo.GetAllByCompanyAsync(request.CompanyId, cancellationToken);

        return departments.Select(d => new DepartmentDto(
            d.Id,
            d.CompanyId,
            d.Name,
            d.Code,
            d.IsActive,
            d.CreatedAt,
            d.UpdatedAt
        )).ToList();
    }
}
