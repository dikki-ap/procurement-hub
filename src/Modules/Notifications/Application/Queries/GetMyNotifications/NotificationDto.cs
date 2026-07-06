namespace ProcureHub.Modules.Notifications.Application.Queries.GetMyNotifications;

public record NotificationDto(
    Guid      Id,
    string    Title,
    string    Message,
    string?   Link,
    bool      IsRead,
    DateTime  CreatedAt,
    DateTime? ReadAt
);
