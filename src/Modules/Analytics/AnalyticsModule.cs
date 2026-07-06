using Microsoft.Extensions.DependencyInjection;

namespace ProcureHub.Modules.Analytics;

public static class AnalyticsModule
{
    public static IServiceCollection AddAnalyticsModule(this IServiceCollection services)
        => services; // Handlers registered globally via AddMediatR in Program.cs
}
