namespace ProcureHub.Modules.CompanyManagement.Application.DTOs;

public record CompanyDto(
    Guid     Id,
    string   Name,
    string   Code,
    string   Type,
    string?  Address,
    string?  Phone,
    string?  Email,
    string?  LogoUrl,
    bool     IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
