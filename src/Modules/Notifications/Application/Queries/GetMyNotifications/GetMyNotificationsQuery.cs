using MediatR;

namespace ProcureHub.Modules.Notifications.Application.Queries.GetMyNotifications;

public record GetMyNotificationsQuery(Guid UserId) : IRequest<List<NotificationDto>>;
