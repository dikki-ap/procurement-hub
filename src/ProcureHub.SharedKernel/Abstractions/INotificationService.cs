namespace ProcureHub.SharedKernel.Abstractions;

public interface INotificationService
{
    Task SendAsync(Guid userId, string title, string message, string? link = null, CancellationToken ct = default);
}
