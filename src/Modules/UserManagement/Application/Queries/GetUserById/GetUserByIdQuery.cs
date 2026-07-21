using ProcureHub.Modules.UserManagement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.UserManagement.Application.Queries.GetUserById;

public record GetUserByIdQuery(Guid Id) : IQuery<UserDto>;
