namespace ProcureHub.Modules.Audit.Application.Queries.GetAuditLog;

public record AuditLogDto(
    Guid     Id,
    Guid?    UserId,
    string?  UserEmail,
    string?  UserFullName,
    string   EntityType,
    Guid     EntityId,
    string   Action,
    string?  BeforeValues,
    string   AfterValues,
    string?  ChangedColumns,
    string?  IpAddress,
    string?  CorrelationId,
    DateTime CreatedAt
);

public record PagedAuditLogDto(
    IReadOnlyList<AuditLogDto> Items,
    int Total,
    int Page,
    int PageSize,
    int TotalPages
);
