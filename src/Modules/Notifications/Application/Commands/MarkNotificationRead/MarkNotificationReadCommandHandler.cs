using MediatR;
using ProcureHub.Modules.Notifications.Domain.Repositories;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Notifications.Application.Commands.MarkNotificationRead;

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Unit>
{
    private readonly IInAppNotificationRepository _repo;

    public MarkNotificationReadCommandHandler(IInAppNotificationRepository repo)
        => _repo = repo;

    public async Task<Unit> Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var notification = await _repo.GetByIdAsync(request.NotificationId, ct)
            ?? throw new NotFoundException("Notification", request.NotificationId);

        if (notification.UserId != request.UserId)
            throw new ForbiddenException("You can only mark your own notifications as read.");

        notification.MarkAsRead();
        await _repo.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
