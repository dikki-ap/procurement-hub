namespace ProcureHub.Modules.UserManagement.Application.DTOs;

public record UserDto(
    Guid     Id,
    Guid     CompanyId,
    string   KeycloakId,
    string   Email,
    string   FullName,
    string   Role,
    Guid?    DepartmentId,
    bool     IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
