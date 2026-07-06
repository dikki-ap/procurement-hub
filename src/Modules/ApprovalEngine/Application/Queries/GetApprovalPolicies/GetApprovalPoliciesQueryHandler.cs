using ProcureHub.Modules.ApprovalEngine.Application.DTOs;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.ApprovalEngine.Application.Queries.GetApprovalPolicies;

public class GetApprovalPoliciesQueryHandler : IQueryHandler<GetApprovalPoliciesQuery, List<ApprovalPolicyDto>>
{
    private readonly IApprovalPolicyRepository _repo;

    public GetApprovalPoliciesQueryHandler(IApprovalPolicyRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<ApprovalPolicyDto>> Handle(GetApprovalPoliciesQuery request, CancellationToken ct)
    {
        var policies = await _repo.GetByCompanyAsync(request.CompanyId, ct);

        return policies.Select(p => new ApprovalPolicyDto(
            p.Id, p.CompanyId, p.ReferenceType, p.Name, p.MinValue, p.MaxValue,
            p.RequiredLevels, p.IsStrategicOverride, p.IsSingleSourceOverride, p.IsActive)).ToList();
    }
}
