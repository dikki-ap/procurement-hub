using MediatR;

namespace ProcureHub.Modules.Notifications.Application.Commands.MarkNotificationRead;

public record MarkNotificationReadCommand(Guid NotificationId, Guid UserId) : IRequest<Unit>;
