using Microsoft.Extensions.DependencyInjection;
using ProcureHub.Modules.MasterData.Application.Services;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.Modules.MasterData.Infrastructure.Jobs;
using ProcureHub.Modules.MasterData.Infrastructure.Repositories;
using ProcureHub.Modules.MasterData.Infrastructure.Services;

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

        services.AddHttpClient<IExchangeRateService, ExchangeRateService>(client =>
        {
            client.BaseAddress = new Uri("https://cdn.jsdelivr.net/npm/@fawazahmed0/currency-api@latest/v1/currencies/");
            client.Timeout     = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        services.AddScoped<IExchangeRateConfigRepository, ExchangeRateConfigRepository>();
        services.AddScoped<IExchangeRateConfigService,    ExchangeRateConfigService>();
        services.AddScoped<ExchangeRateJob>();

        return services;
    }
}
