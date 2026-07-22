using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.Notifications.Application.Commands.MarkNotificationRead;
using ProcureHub.Modules.Notifications.Application.Queries.GetMyNotifications;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.Notifications;

/// <summary>In-app notifications — supports both internal and vendor users.</summary>
[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator           _mediator;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
    }

    /// <summary>Get the current user's notifications (latest 50).</summary>
    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        List<NotificationDto> result;

        if (_currentUser.VendorUserId is { } vendorUserId)
        {
            result = await _mediator.Send(new GetMyNotificationsQuery(vendorUserId, IsVendorUser: true), ct);
        }
        else
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedAccessException("User not authenticated.");
            result = await _mediator.Send(new GetMyNotificationsQuery(userId, IsVendorUser: false), ct);
        }

        return Ok(ApiResponse<List<NotificationDto>>.Ok(result));
    }

    /// <summary>Mark a notification as read.</summary>
    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? _currentUser.VendorUserId
            ?? throw new UnauthorizedAccessException("User not authenticated.");

        await _mediator.Send(new MarkNotificationReadCommand(id, userId), ct);
        return Ok(ApiResponse.Ok());
    }
}
