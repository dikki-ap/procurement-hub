using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace ProcureHub.SharedKernel.Audit;

public class AuditEntry
{
    public AuditEntry(EntityEntry entry) => Entry = entry;

    public EntityEntry Entry { get; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserFullName { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public Dictionary<string, object?> BeforeValues { get; } = [];
    public Dictionary<string, object?> AfterValues { get; } = [];
    public List<string> ChangedColumns { get; } = [];
    public List<PropertyEntry> TemporaryProperties { get; } = [];
    public bool HasTemporaryProperties => TemporaryProperties.Count > 0;

    public AuditLog ToAuditLog(
        Guid? userId,
        string? userEmail,
        string? userFullName,
        string? ipAddress,
        string? userAgent,
        string? correlationId)
    {
        // Resolve DB-generated values that were not available before SaveChanges
        foreach (var prop in TemporaryProperties)
        {
            if (prop.Metadata.IsPrimaryKey() && prop.CurrentValue is Guid id)
                EntityId = id;
            else
                AfterValues[prop.Metadata.Name] = prop.CurrentValue;
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return new AuditLog
        {
            UserId         = userId,
            UserEmail      = userEmail,
            UserFullName   = userFullName,
            EntityType     = EntityType,
            EntityId       = EntityId,
            Action         = Action,
            BeforeValues   = BeforeValues.Count > 0
                ? JsonSerializer.Serialize(BeforeValues, options) : null,
            AfterValues    = JsonSerializer.Serialize(AfterValues, options),
            ChangedColumns = ChangedColumns.Count > 0
                ? JsonSerializer.Serialize(ChangedColumns, options) : null,
            IpAddress      = ipAddress,
            UserAgent      = userAgent,
            CorrelationId  = correlationId,
            CreatedAt      = DateTime.UtcNow
        };
    }
}
