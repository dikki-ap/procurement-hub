namespace ProcureHub.Modules.MasterData.Application.Services;

public interface IExchangeRateConfigService
{
    Task<bool> GetAutoSyncAsync(CancellationToken ct = default);
    Task SetAutoSyncAsync(bool autoSync, CancellationToken ct = default);
}
