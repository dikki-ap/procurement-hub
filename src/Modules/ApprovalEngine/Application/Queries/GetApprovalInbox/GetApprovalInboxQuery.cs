using ProcureHub.Modules.ApprovalEngine.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.ApprovalEngine.Application.Queries.GetApprovalInbox;

public record GetApprovalInboxQuery(Guid UserId, Guid CompanyId) : IQuery<List<ApprovalInboxItemDto>>;
