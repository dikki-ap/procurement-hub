using ProcureHub.Modules.MasterData.Domain.Entities;

namespace ProcureHub.Modules.MasterData.Domain.Repositories;

public interface IExchangeRateConfigRepository
{
    Task<ExchangeRateConfig> GetAsync(CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
