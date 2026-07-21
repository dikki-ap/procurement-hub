using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.CompanyManagement.Application.Commands.UpdateCompany;

public record UpdateCompanyCommand(
    Guid    CompanyId,
    string  Name,
    string  Type,
    string? Address,
    string? Phone,
    string? Email
) : ICommand;
