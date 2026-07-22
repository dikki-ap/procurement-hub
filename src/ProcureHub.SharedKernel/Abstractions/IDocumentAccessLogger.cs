namespace ProcureHub.SharedKernel.Abstractions;

public interface IDocumentAccessLogger
{
    Task LogAsync(
        string  entityType,
        Guid    entityId,
        string? fileName,
        bool    inline,
        CancellationToken ct = default);
}
