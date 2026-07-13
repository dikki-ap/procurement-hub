using Microsoft.Extensions.Logging;
using ProcureHub.Modules.MasterData.Application.Services;

namespace ProcureHub.Modules.MasterData.Infrastructure.Jobs;

public class ExchangeRateJob
{
    private readonly IExchangeRateService        _service;
    private readonly ILogger<ExchangeRateJob>    _logger;

    public ExchangeRateJob(IExchangeRateService service, ILogger<ExchangeRateJob> logger)
    {
        _service = service;
        _logger  = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Exchange rate sync job started.");
        await _service.SyncRatesAsync(ct);
        _logger.LogInformation("Exchange rate sync job completed.");
    }
}
