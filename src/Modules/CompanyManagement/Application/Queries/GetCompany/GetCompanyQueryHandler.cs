using ProcureHub.Modules.CompanyManagement.Application.DTOs;
using ProcureHub.Modules.CompanyManagement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.CompanyManagement.Application.Queries.GetCompany;

public class GetCompanyQueryHandler : IQueryHandler<GetCompanyQuery, CompanyDto>
{
    private readonly ICompanyRepository _repo;

    public GetCompanyQueryHandler(ICompanyRepository repo) => _repo = repo;

    public async Task<CompanyDto> Handle(GetCompanyQuery request, CancellationToken cancellationToken)
    {
        var company = await _repo.GetByIdAsync(request.CompanyId, cancellationToken)
            ?? throw new NotFoundException("Company", request.CompanyId);

        return new CompanyDto(
            company.Id,
            company.Name,
            company.Code,
            company.Type,
            company.Address,
            company.Phone,
            company.Email,
            company.LogoUrl,
            company.IsActive,
            company.CreatedAt,
            company.UpdatedAt
        );
    }
}
