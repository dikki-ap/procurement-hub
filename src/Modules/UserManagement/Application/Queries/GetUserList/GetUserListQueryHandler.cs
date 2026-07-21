using ProcureHub.Modules.UserManagement.Application.DTOs;
using ProcureHub.Modules.UserManagement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.UserManagement.Application.Queries.GetUserList;

public class GetUserListQueryHandler : IQueryHandler<GetUserListQuery, List<UserDto>>
{
    private readonly IUserRepository _repo;

    public GetUserListQueryHandler(IUserRepository repo) => _repo = repo;

    public async Task<List<UserDto>> Handle(GetUserListQuery request, CancellationToken cancellationToken)
    {
        var users = await _repo.GetAllByCompanyAsync(request.CompanyId, cancellationToken);

        return users.Select(u => new UserDto(
            u.Id,
            u.CompanyId,
            u.KeycloakId,
            u.Email,
            u.FullName,
            u.Role,
            u.DepartmentId,
            u.IsActive,
            u.CreatedAt,
            u.UpdatedAt
        )).ToList();
    }
}
