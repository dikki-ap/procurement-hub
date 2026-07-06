using ProcureHub.Modules.ApprovalEngine.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.ApprovalEngine.Application.Queries.GetApprovalWorkflow;

public record GetApprovalWorkflowQuery(Guid WorkflowId) : IQuery<ApprovalWorkflowDto>;
