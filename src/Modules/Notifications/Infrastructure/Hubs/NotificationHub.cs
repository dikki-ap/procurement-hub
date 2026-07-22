using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using ProcureHub.SharedKernel.Abstractions;

namespace ProcureHub.Modules.Notifications.Infrastructure.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ICurrentUserService      _currentUser;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ICurrentUserService currentUser, ILogger<NotificationHub> logger)
    {
        _currentUser = currentUser;
        _logger      = logger;
    }

    public override async Task OnConnectedAsync()
    {
        if (_currentUser.VendorUserId is { } vendorUserId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"vendor-user:{vendorUserId}");
            _logger.LogInformation("Vendor user {VendorUserId} connected: {ConnectionId}", vendorUserId, Context.ConnectionId);
        }
        else if (_currentUser.UserId is { } userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
            _logger.LogInformation("Internal user {UserId} connected: {ConnectionId}", userId, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_currentUser.VendorUserId is { } vendorUserId)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"vendor-user:{vendorUserId}");
        else if (_currentUser.UserId is { } userId)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");

        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
