namespace ProcureHub.Modules.MasterData.Application.Services;

public interface IExchangeRateService
{
    Task SyncRatesAsync(CancellationToken ct = default);
}
