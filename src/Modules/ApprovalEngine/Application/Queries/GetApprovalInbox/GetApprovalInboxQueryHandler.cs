using ProcureHub.Modules.ApprovalEngine.Application.DTOs;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.ApprovalEngine.Application.Queries.GetApprovalInbox;

public class GetApprovalInboxQueryHandler : IQueryHandler<GetApprovalInboxQuery, List<ApprovalInboxItemDto>>
{
    private readonly IApprovalWorkflowRepository _repo;

    public GetApprovalInboxQueryHandler(IApprovalWorkflowRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<ApprovalInboxItemDto>> Handle(GetApprovalInboxQuery request, CancellationToken ct)
    {
        var workflows = await _repo.GetInboxAsync(request.UserId, request.CompanyId, ct);

        return workflows.Select(w => new ApprovalInboxItemDto(
            w.Id,
            w.ReferenceType,
            w.ReferenceNumber,
            w.ReferenceTitle,
            w.TotalValue,
            w.CurrentLevel,
            w.MaxLevel,
            w.Status,
            w.CreatedAt)).ToList();
    }
}
