using MediatR;
using ProcureHub.Modules.CompanyManagement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.CompanyManagement.Application.Commands.UpdateCompany;

public class UpdateCompanyCommandHandler : ICommandHandler<UpdateCompanyCommand>
{
    private readonly ICompanyRepository _repo;

    public UpdateCompanyCommandHandler(ICompanyRepository repo) => _repo = repo;

    public async Task<Unit> Handle(UpdateCompanyCommand command, CancellationToken ct)
    {
        var company = await _repo.GetByIdAsync(command.CompanyId, ct)
            ?? throw new NotFoundException("Company", command.CompanyId);

        company.Name    = command.Name;
        company.Type    = command.Type;
        company.Address = command.Address;
        company.Phone   = command.Phone;
        company.Email   = command.Email;

        _repo.Update(company);
        await _repo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
