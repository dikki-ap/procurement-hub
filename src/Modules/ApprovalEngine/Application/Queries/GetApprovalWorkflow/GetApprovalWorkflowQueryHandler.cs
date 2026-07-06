using ProcureHub.Modules.ApprovalEngine.Application.DTOs;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.ApprovalEngine.Application.Queries.GetApprovalWorkflow;

public class GetApprovalWorkflowQueryHandler : IQueryHandler<GetApprovalWorkflowQuery, ApprovalWorkflowDto>
{
    private readonly IApprovalWorkflowRepository _repo;

    public GetApprovalWorkflowQueryHandler(IApprovalWorkflowRepository repo)
    {
        _repo = repo;
    }

    public async Task<ApprovalWorkflowDto> Handle(GetApprovalWorkflowQuery request, CancellationToken ct)
    {
        var w = await _repo.GetByIdWithDetailsAsync(request.WorkflowId, ct)
                ?? throw new NotFoundException("ApprovalWorkflow", request.WorkflowId);

        return new ApprovalWorkflowDto(
            w.Id,
            w.ReferenceType,
            w.ReferenceId,
            w.ReferenceNumber,
            w.ReferenceTitle,
            w.TotalValue,
            w.CurrentLevel,
            w.MaxLevel,
            w.Status,
            w.Iteration,
            w.CompletedAt,
            w.CreatedAt,
            w.History
                .OrderByDescending(h => h.ActedAt)
                .Select(h => new ApprovalHistoryDto(h.Id, h.Level, h.Action, h.ActorName, h.Reason, h.ActedAt))
                .ToList(),
            w.Assignments
                .Select(a => new ApproverAssignmentDto(a.AssignedUserId, a.AssignedUserName, a.Level, a.IsDelegate))
                .ToList());
    }
}
