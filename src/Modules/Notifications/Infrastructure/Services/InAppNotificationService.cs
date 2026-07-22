using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ProcureHub.Modules.Notifications.Infrastructure.Hubs;
using ProcureHub.Modules.Notifications.Domain;
using ProcureHub.Modules.Notifications.Domain.Repositories;
using ProcureHub.SharedKernel.Abstractions;

namespace ProcureHub.Modules.Notifications.Infrastructure.Services;

public class InAppNotificationService : INotificationService
{
    private readonly IInAppNotificationRepository      _repo;
    private readonly IHubContext<NotificationHub>      _hub;
    private readonly ILogger<InAppNotificationService> _logger;

    public InAppNotificationService(
        IInAppNotificationRepository      repo,
        IHubContext<NotificationHub>      hub,
        ILogger<InAppNotificationService> logger)
    {
        _repo   = repo;
        _hub    = hub;
        _logger = logger;
    }

    public async Task SendAsync(
        Guid userId, string title, string message,
        string? link = null, CancellationToken ct = default)
    {
        var notification = new InAppNotification
        {
            UserId  = userId,
            Title   = title,
            Message = message,
            Link    = link,
        };

        _repo.Add(notification);
        await _repo.SaveChangesAsync(ct);

        await _hub.Clients.Group($"user:{userId}").SendAsync(
            "ReceiveNotification",
            new { notification.Id, notification.Title, notification.Message, notification.Link, notification.CreatedAt },
            ct);

        _logger.LogInformation("In-app notification sent to user {UserId}: {Title}", userId, title);
    }

    public async Task SendToVendorUserAsync(
        Guid vendorUserId, string title, string message,
        string? link = null, CancellationToken ct = default)
    {
        var notification = new InAppNotification
        {
            UserId       = Guid.Empty, // not an internal user
            VendorUserId = vendorUserId,
            Title        = title,
            Message      = message,
            Link         = link,
        };

        _repo.Add(notification);
        await _repo.SaveChangesAsync(ct);

        await _hub.Clients.Group($"vendor-user:{vendorUserId}").SendAsync(
            "ReceiveNotification",
            new { notification.Id, notification.Title, notification.Message, notification.Link, notification.CreatedAt },
            ct);

        _logger.LogInformation("In-app notification sent to vendor user {VendorUserId}: {Title}", vendorUserId, title);
    }
}
