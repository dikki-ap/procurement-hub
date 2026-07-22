using MediatR;
using ProcureHub.Modules.Notifications.Domain.Repositories;

namespace ProcureHub.Modules.Notifications.Application.Queries.GetMyNotifications;

public class GetMyNotificationsQueryHandler
    : IRequestHandler<GetMyNotificationsQuery, List<NotificationDto>>
{
    private readonly IInAppNotificationRepository _repo;

    public GetMyNotificationsQueryHandler(IInAppNotificationRepository repo)
        => _repo = repo;

    public async Task<List<NotificationDto>> Handle(GetMyNotificationsQuery request, CancellationToken ct)
    {
        var list = request.IsVendorUser
            ? await _repo.GetForVendorUserAsync(request.UserId, ct)
            : await _repo.GetForUserAsync(request.UserId, ct);

        return list
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.Link, n.IsRead, n.CreatedAt, n.ReadAt))
            .ToList();
    }
}
