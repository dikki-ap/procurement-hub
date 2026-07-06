using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureHub.Modules.Notifications.Domain.Repositories;
using ProcureHub.Modules.Notifications.Infrastructure.Repositories;
using ProcureHub.Modules.Notifications.Infrastructure.Services;
using ProcureHub.Modules.Notifications.Infrastructure.Settings;
using ProcureHub.SharedKernel.Abstractions;

namespace ProcureHub.Modules.Notifications;

public static class NotificationsModule
{
    public static IServiceCollection AddNotificationsModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("Email"));

        services.AddScoped<IInAppNotificationRepository, InAppNotificationRepository>();
        services.AddScoped<IEmailService,        EmailService>();
        services.AddScoped<INotificationService, InAppNotificationService>();

        return services;
    }
}
