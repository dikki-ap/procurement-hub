using UUIDNext;

namespace ProcureHub.SharedKernel.Audit;

public class AuditLog
{
    public Guid Id { get; set; } = Uuid.NewSequential();
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserFullName { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;

    /// <summary>JSON snapshot before the change. NULL for Created actions.</summary>
    public string? BeforeValues { get; set; }

    /// <summary>JSON snapshot after the change.</summary>
    public string AfterValues { get; set; } = string.Empty;

    /// <summary>JSON array of column names that were modified. NULL for Created/Deleted.</summary>
    public string? ChangedColumns { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
