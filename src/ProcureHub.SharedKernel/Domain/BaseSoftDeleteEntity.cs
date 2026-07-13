namespace ProcureHub.SharedKernel.Domain;

/// <summary>
/// Entities that support soft delete.
/// When IsDeleted = true, EF Core global query filter excludes the record automatically.
/// Records are never physically removed from the database.
/// </summary>
public abstract class BaseSoftDeleteEntity : BaseAuditableEntity
{
    public bool IsDeleted { get; set; } = false;

    /// <summary>UTC timestamp when the record was soft-deleted.</summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>Internal DB user UUID who performed the delete (FK → users.id).</summary>
    public Guid? DeletedById { get; set; }

    public User? DeletedBy { get; set; }

    public void SoftDelete(Guid? deletedByUserId = null)
    {
        IsDeleted   = true;
        DeletedAt   = DateTime.UtcNow;
        DeletedById = deletedByUserId;
    }

    public void Restore(Guid restoredByUserId)
    {
        IsDeleted   = false;
        DeletedAt   = null;
        DeletedById = null;
        UpdatedById = restoredByUserId;
        UpdatedAt   = DateTime.UtcNow;
    }
}
