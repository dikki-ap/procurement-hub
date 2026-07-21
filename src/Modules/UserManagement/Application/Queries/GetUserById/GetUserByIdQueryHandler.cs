using ProcureHub.Modules.UserManagement.Application.DTOs;
using ProcureHub.Modules.UserManagement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.UserManagement.Application.Queries.GetUserById;

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _repo;

    public GetUserByIdQueryHandler(IUserRepository repo) => _repo = repo;

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _repo.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("User", request.Id);

        return new UserDto(
            user.Id,
            user.CompanyId,
            user.KeycloakId,
            user.Email,
            user.FullName,
            user.Role,
            user.DepartmentId,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt
        );
    }
}
