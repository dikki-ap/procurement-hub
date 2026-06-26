using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ProcureHub.SharedKernel.Abstractions;

namespace ProcureHub.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ICurrentUserService currentUser, ILogger<NotificationHub> logger)
    {
        _currentUser = currentUser;
        _logger      = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _currentUser.UserId?.ToString();
        if (userId is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");

        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = _currentUser.UserId?.ToString();
        if (userId is not null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");

        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
