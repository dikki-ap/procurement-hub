using Microsoft.Extensions.DependencyInjection;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.Modules.MasterData.Infrastructure.Repositories;

namespace ProcureHub.Modules.MasterData;

public static class MasterDataModule
{
    public static IServiceCollection AddMasterDataServices(this IServiceCollection services)
    {
        services.AddScoped<IMaterialCategoryRepository, MaterialCategoryRepository>();
        services.AddScoped<IMaterialRepository,         MaterialRepository>();
        services.AddScoped<IUnitOfMeasureRepository,    UnitOfMeasureRepository>();
        services.AddScoped<ICurrencyRepository,         CurrencyRepository>();
        services.AddScoped<IPaymentTermRepository,      PaymentTermRepository>();
        services.AddScoped<ILocationRepository,         LocationRepository>();
        services.AddScoped<IDocumentTypeRepository,     DocumentTypeRepository>();

        return services;
    }
}
