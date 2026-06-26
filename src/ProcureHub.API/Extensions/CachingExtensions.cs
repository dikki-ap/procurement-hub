using ProcureHub.SharedKernel.Caching;

namespace ProcureHub.API.Extensions;

public static class CachingExtensions
{
    public static IServiceCollection AddApplicationCaching(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        return services;
    }
}
