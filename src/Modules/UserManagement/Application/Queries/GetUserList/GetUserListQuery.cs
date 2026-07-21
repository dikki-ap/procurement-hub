using ProcureHub.Modules.UserManagement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.UserManagement.Application.Queries.GetUserList;

public record GetUserListQuery(Guid CompanyId) : IQuery<List<UserDto>>;
