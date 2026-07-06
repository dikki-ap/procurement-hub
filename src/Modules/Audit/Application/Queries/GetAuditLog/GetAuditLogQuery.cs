using MediatR;

namespace ProcureHub.Modules.Audit.Application.Queries.GetAuditLog;

public record GetAuditLogQuery(
    string?   EntityType = null,
    Guid?     EntityId   = null,
    Guid?     UserId     = null,
    string?   Action     = null,
    DateTime? From       = null,
    DateTime? To         = null,
    int       Page       = 1,
    int       PageSize   = 50
) : IRequest<PagedAuditLogDto>;
