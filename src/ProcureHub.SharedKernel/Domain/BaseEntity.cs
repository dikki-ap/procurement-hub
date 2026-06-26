using UUIDNext;

namespace ProcureHub.SharedKernel.Domain;

public abstract class BaseEntity
{
    // UUID v7: time-ordered, globally unique, safe to expose in public URLs.
    // .NET 8 does not support Guid.CreateVersion7() natively — UUIDNext is required.
    public Guid Id { get; protected set; } = Uuid.NewSequential();
}
