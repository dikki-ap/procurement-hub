namespace ProcureHub.Modules.Notifications.Domain.Repositories;

public interface IInAppNotificationRepository
{
    Task<List<InAppNotification>> GetForUserAsync(Guid userId, CancellationToken ct = default);
    Task<List<InAppNotification>> GetForVendorUserAsync(Guid vendorUserId, CancellationToken ct = default);
    Task<InAppNotification?>      GetByIdAsync(Guid id, CancellationToken ct = default);
    void                          Add(InAppNotification notification);
    Task<int>                     SaveChangesAsync(CancellationToken ct = default);
}
