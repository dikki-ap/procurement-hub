using System.Text.Json;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Audit;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.API.Services;

public class DocumentAccessLogger : IDocumentAccessLogger
{
    private readonly ApplicationDbContext  _db;
    private readonly ICurrentUserService   _currentUser;

    public DocumentAccessLogger(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db          = db;
        _currentUser = currentUser;
    }

    public async Task LogAsync(
        string  entityType,
        Guid    entityId,
        string? fileName,
        bool    inline,
        CancellationToken ct = default)
    {
        var entry = new AuditLog
        {
            UserId       = _currentUser.UserId ?? _currentUser.VendorUserId,
            UserEmail    = _currentUser.Email,
            UserFullName = _currentUser.FullName,
            EntityType   = entityType,
            EntityId     = entityId,
            Action       = inline ? "DocumentPreview" : "DocumentDownload",
            AfterValues  = JsonSerializer.Serialize(new { fileName, inline }),
            IpAddress    = _currentUser.IpAddress,
            UserAgent    = _currentUser.UserAgent,
        };

        _db.Set<AuditLog>().Add(entry);
        await _db.SaveChangesAsync(ct);
    }
}
