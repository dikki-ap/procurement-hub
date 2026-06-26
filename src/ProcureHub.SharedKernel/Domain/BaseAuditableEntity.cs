namespace ProcureHub.SharedKernel.Domain;

/// <summary>
/// All timestamps are stored as UTC in the database.
/// Frontend converts to local timezone using date-fns-tz.
/// CreatedById and UpdatedById store the internal users table UUID, not the Keycloak UUID.
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    /// <summary>UTC timestamp when the record was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Internal DB user UUID who created this record (FK → users.id).
    /// NULL when created by the system or an anonymous actor such as vendor self-registration.
    /// </summary>
    public Guid? CreatedById { get; set; }

    /// <summary>UTC timestamp when the record was last updated.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Internal DB user UUID who last updated this record (FK → users.id).</summary>
    public Guid? UpdatedById { get; set; }

    public User? CreatedBy { get; set; }
    public User? UpdatedBy { get; set; }
}
