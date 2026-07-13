using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Audit;
using ProcureHub.SharedKernel.Domain;

namespace ProcureHub.SharedKernel.Database;

public class ApplicationDbContext : DbContext
{
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // ── Core ──────────────────────────────────────────────────
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();

    // ── Audit ─────────────────────────────────────────────────
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUser,
        IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _currentUser         = currentUser;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Load entity configurations from all loaded assemblies (modular monolith pattern)
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try { modelBuilder.ApplyConfigurationsFromAssembly(assembly); }
            catch { /* skip assemblies without configurations */ }
        }

        // Apply global soft delete filter for all BaseSoftDeleteEntity descendants
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseSoftDeleteEntity).IsAssignableFrom(entityType.ClrType))
            {
                var param     = Expression.Parameter(entityType.ClrType, "e");
                var prop      = Expression.Property(param, nameof(BaseSoftDeleteEntity.IsDeleted));
                var condition = Expression.Equal(prop, Expression.Constant(false));
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(Expression.Lambda(condition, param));
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Capture audit entries BEFORE SaveChanges while OriginalValues are still available
        var auditEntries = CaptureAuditEntries();

        // Apply UTC timestamps and actor UUIDs to auditable entities
        ApplyAuditableFields();

        var result = await base.SaveChangesAsync(ct);

        // Save audit logs after successful save so DB-generated values are resolved
        await SaveAuditLogsAsync(auditEntries, ct);

        return result;
    }

    private static readonly HashSet<string> AuditableEntityNames =
    [
        "Vendor", "VendorDocument", "Material",
        "PurchaseRequisition", "RFQ", "VendorQuotation",
        "PurchaseOrder", "ApprovalWorkflow", "ApprovalHistory",
        "GoodsReceipt", "Invoice", "Contract",
        "ApprovalPolicy", "Currency", "ApproverAssignment",
    ];

    private List<AuditEntry> CaptureAuditEntries()
    {
        ChangeTracker.DetectChanges();

        var userId       = _currentUser.UserId;
        var userEmail    = _currentUser.Email;
        var userFullName = _currentUser.FullName;
        var entries      = new List<AuditEntry>();

        var skipCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CreatedAt", "UpdatedAt", "CreatedById", "UpdatedById",
            "DeletedAt", "DeletedById"
        };

        foreach (var entry in ChangeTracker.Entries())
        {
            var entityName = entry.Entity.GetType().Name;
            if (!AuditableEntityNames.Contains(entityName)) continue;
            if (entry.State is EntityState.Detached or EntityState.Unchanged) continue;

            var auditEntry = new AuditEntry(entry)
            {
                EntityType   = entityName,
                UserId       = userId,
                UserEmail    = userEmail,
                UserFullName = userFullName,
                Action       = entry.State switch
                {
                    EntityState.Added    => "Created",
                    EntityState.Modified => "Updated",
                    EntityState.Deleted  => "Deleted",
                    _                    => "Unknown"
                }
            };

            var pk = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            if (pk?.IsTemporary == true)
                auditEntry.TemporaryProperties.Add(pk);
            else if (pk?.CurrentValue is Guid id)
                auditEntry.EntityId = id;

            foreach (var prop in entry.Properties)
            {
                if (skipCols.Contains(prop.Metadata.Name)) continue;
                if (prop.IsTemporary) { auditEntry.TemporaryProperties.Add(prop); continue; }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.AfterValues[prop.Metadata.Name] = prop.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        auditEntry.BeforeValues[prop.Metadata.Name] = prop.OriginalValue;
                        break;
                    case EntityState.Modified when prop.IsModified:
                        if (!Equals(prop.OriginalValue, prop.CurrentValue))
                        {
                            auditEntry.BeforeValues[prop.Metadata.Name] = prop.OriginalValue;
                            auditEntry.AfterValues[prop.Metadata.Name]  = prop.CurrentValue;
                            auditEntry.ChangedColumns.Add(prop.Metadata.Name);
                        }
                        break;
                }
            }

            if (entry.State == EntityState.Added
                || entry.State == EntityState.Deleted
                || auditEntry.ChangedColumns.Count > 0)
            {
                entries.Add(auditEntry);
            }
        }

        return entries;
    }

    private void ApplyAuditableFields()
    {
        var now    = DateTime.UtcNow;
        var userId = _currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt   = now;
                    entry.Entity.UpdatedAt   = now;
                    entry.Entity.CreatedById = userId;
                    entry.Entity.UpdatedById = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt   = now;
                    entry.Entity.UpdatedById = userId;
                    // Prevent overwriting the original creator
                    entry.Property(e => e.CreatedAt).IsModified  = false;
                    entry.Property(e => e.CreatedById).IsModified = false;
                    break;

                case EntityState.Deleted:
                    // Intercept hard delete and convert to soft delete automatically
                    if (entry.Entity is BaseSoftDeleteEntity sd)
                    {
                        entry.State = EntityState.Modified;
                        sd.SoftDelete(userId); // null when no internal user (e.g. vendor self-service) — FK allows null
                        entry.Entity.UpdatedAt   = now;
                        entry.Entity.UpdatedById = userId;
                    }
                    break;
            }
        }
    }

    private async Task SaveAuditLogsAsync(List<AuditEntry> auditEntries, CancellationToken ct)
    {
        if (auditEntries.Count == 0) return;

        var ipAddress     = _currentUser.IpAddress;
        var userAgent     = _currentUser.UserAgent;
        var correlationId = _httpContextAccessor.HttpContext?
            .Request.Headers["X-Correlation-Id"].FirstOrDefault();

        var logs = auditEntries
            .Select(e => e.ToAuditLog(
                e.UserId, e.UserEmail, e.UserFullName,
                ipAddress, userAgent, correlationId))
            .ToList();

        await AuditLogs.AddRangeAsync(logs, ct);

        // Call base directly to avoid recursive audit capture
        await base.SaveChangesAsync(ct);
    }
}
