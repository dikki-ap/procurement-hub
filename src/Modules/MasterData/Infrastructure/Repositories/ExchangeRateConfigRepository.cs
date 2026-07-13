using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.Modules.MasterData.Infrastructure.Repositories;

public class ExchangeRateConfigRepository : IExchangeRateConfigRepository
{
    private readonly ApplicationDbContext _db;

    public ExchangeRateConfigRepository(ApplicationDbContext db) => _db = db;

    public Task<ExchangeRateConfig> GetAsync(CancellationToken ct = default)
        => _db.Set<ExchangeRateConfig>().FirstAsync(ct);

    public async Task SaveAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}
