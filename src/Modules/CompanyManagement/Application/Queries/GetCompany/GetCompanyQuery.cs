using ProcureHub.Modules.CompanyManagement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.CompanyManagement.Application.Queries.GetCompany;

public record GetCompanyQuery(Guid CompanyId) : IQuery<CompanyDto>;
