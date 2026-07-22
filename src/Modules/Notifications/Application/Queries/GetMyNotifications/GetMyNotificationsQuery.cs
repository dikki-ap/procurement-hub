using MediatR;

namespace ProcureHub.Modules.Notifications.Application.Queries.GetMyNotifications;

public record GetMyNotificationsQuery(Guid UserId, bool IsVendorUser = false) : IRequest<List<NotificationDto>>;
