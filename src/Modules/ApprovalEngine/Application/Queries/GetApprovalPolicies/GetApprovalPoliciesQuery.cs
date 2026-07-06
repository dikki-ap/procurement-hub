using ProcureHub.Modules.ApprovalEngine.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.ApprovalEngine.Application.Queries.GetApprovalPolicies;

public record GetApprovalPoliciesQuery(Guid CompanyId) : IQuery<List<ApprovalPolicyDto>>;
