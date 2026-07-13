using ProcureHub.Modules.MasterData.Application.Services;
using ProcureHub.Modules.MasterData.Domain.Repositories;

namespace ProcureHub.Modules.MasterData.Infrastructure.Services;

public class ExchangeRateConfigService : IExchangeRateConfigService
{
    private readonly IExchangeRateConfigRepository _repo;

    public ExchangeRateConfigService(IExchangeRateConfigRepository repo) => _repo = repo;

    public async Task<bool> GetAutoSyncAsync(CancellationToken ct = default)
    {
        var config = await _repo.GetAsync(ct);
        return config.AutoSync;
    }

    public async Task SetAutoSyncAsync(bool autoSync, CancellationToken ct = default)
    {
        var config = await _repo.GetAsync(ct);
        config.AutoSync = autoSync;
        await _repo.SaveAsync(ct);
    }
}
