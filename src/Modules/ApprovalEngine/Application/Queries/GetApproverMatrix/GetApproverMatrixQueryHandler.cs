using ProcureHub.Modules.ApprovalEngine.Application.DTOs;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.ApprovalEngine.Application.Queries.GetApproverMatrix;

public class GetApproverMatrixQueryHandler : IQueryHandler<GetApproverMatrixQuery, List<ApproverMatrixEntryDto>>
{
    private readonly IApproverMatrixRepository _repo;

    public GetApproverMatrixQueryHandler(IApproverMatrixRepository repo) => _repo = repo;

    public async Task<List<ApproverMatrixEntryDto>> Handle(GetApproverMatrixQuery request, CancellationToken ct)
    {
        var entries = await _repo.GetAllByCompanyAsync(request.CompanyId, ct);

        return entries.Select(e => new ApproverMatrixEntryDto(
            e.Id,
            e.ReferenceType,
            e.Level,
            e.Name,
            e.Position,
            e.Email,
            e.CreatedBy?.FullName,
            e.CreatedAt,
            e.UpdatedBy?.FullName,
            e.UpdatedAt)).ToList();
    }
}
