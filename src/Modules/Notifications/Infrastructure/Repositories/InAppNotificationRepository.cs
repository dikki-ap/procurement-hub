using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.Notifications.Domain;
using ProcureHub.Modules.Notifications.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.Notifications.Infrastructure.Repositories;

public class InAppNotificationRepository : IInAppNotificationRepository
{
    private readonly ApplicationDbContext _db;

    public InAppNotificationRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<InAppNotification>> GetForUserAsync(Guid userId, CancellationToken ct = default)
        => await _db.Set<InAppNotification>()
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(50)
                    .ToListAsync(ct);

    public async Task<InAppNotification?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Set<InAppNotification>().FindAsync([id], ct);

    public void Add(InAppNotification notification) => _db.Set<InAppNotification>().Add(notification);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
