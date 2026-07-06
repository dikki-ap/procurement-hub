using MediatR;

namespace ProcureHub.Modules.Audit.Application.Commands.ExportAuditLog;

public record ExportAuditLogCommand(
    string?   EntityType = null,
    Guid?     EntityId   = null,
    Guid?     UserId     = null,
    string?   Action     = null,
    DateTime? From       = null,
    DateTime? To         = null
) : IRequest<string>; // returns presigned download URL
