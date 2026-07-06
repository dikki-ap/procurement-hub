using Microsoft.Extensions.DependencyInjection;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.Modules.VendorManagement.Infrastructure.Jobs;
using ProcureHub.Modules.VendorManagement.Infrastructure.Repositories;
using ProcureHub.Modules.VendorManagement.Infrastructure.Storage;

namespace ProcureHub.Modules.VendorManagement;

public static class VendorManagementModule
{
    public static IServiceCollection AddVendorManagementServices(this IServiceCollection services)
    {
        services.AddScoped<IVendorRepository,         VendorRepository>();
        services.AddScoped<IVendorDocumentRepository, VendorDocumentRepository>();
        services.AddScoped<IVendorScoreRepository,    VendorScoreRepository>();

        services.AddScoped<IStorageService,           SeaweedFsStorageService>();

        services.AddHostedService<SeaweedFsBucketInitializer>();

        services.AddScoped<DocumentExpiryCheckJob>();

        return services;
    }
}
