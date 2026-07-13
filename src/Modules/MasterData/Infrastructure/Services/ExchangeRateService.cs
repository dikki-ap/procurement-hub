using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProcureHub.Modules.MasterData.Application.Services;
using ProcureHub.Modules.MasterData.Domain.Repositories;
using ProcureHub.SharedKernel.Caching;

namespace ProcureHub.Modules.MasterData.Infrastructure.Services;

/// <summary>
/// Fetches live exchange rates from fawazahmed0/exchange-api (jsDelivr CDN).
/// Source: 170+ currencies including IDR, updated daily, no API key required.
/// ExchangeRate semantics: "1 unit of this currency = ExchangeRate units of base currency".
/// API returns 1 base = X other, so we store 1/rate (inverse).
/// </summary>
public class ExchangeRateService : IExchangeRateService
{
    private readonly ICurrencyRepository          _repo;
    private readonly HttpClient                   _http;
    private readonly ICacheService                _cache;
    private readonly ILogger<ExchangeRateService> _logger;

    public ExchangeRateService(
        ICurrencyRepository          repo,
        HttpClient                   http,
        ICacheService                cache,
        ILogger<ExchangeRateService> logger)
    {
        _repo   = repo;
        _http   = http;
        _cache  = cache;
        _logger = logger;
    }

    public async Task SyncRatesAsync(CancellationToken ct = default)
    {
        var all     = await _repo.GetAllAsync(ct);
        var base_   = all.FirstOrDefault(c => c.IsBase);
        var nonBase = all.Where(c => !c.IsBase && c.IsActive).ToList();

        if (base_ is null || nonBase.Count == 0)
        {
            _logger.LogWarning("Exchange rate sync skipped: no base currency or no active non-base currencies.");
            return;
        }

        var baseCode = base_.Code.ToLower();
        var url      = $"{baseCode}.json";

        try
        {
            var root = await _http.GetFromJsonAsync<JsonDocument>(url, ct);
            if (root is null || !root.RootElement.TryGetProperty(baseCode, out var ratesElement))
            {
                _logger.LogWarning("Exchange rate sync: unexpected response format from provider.");
                return;
            }

            var rates = ratesElement.Deserialize<Dictionary<string, double>>();
            if (rates is null)
            {
                _logger.LogWarning("Exchange rate sync: could not deserialize rates.");
                return;
            }

            var date    = root.RootElement.TryGetProperty("date", out var d) ? d.GetString() : "unknown";
            var now     = DateTime.UtcNow;
            var updated = 0;

            foreach (var currency in nonBase)
            {
                if (!rates.TryGetValue(currency.Code.ToLower(), out var rate) || rate <= 0)
                    continue;

                currency.ExchangeRate  = Math.Round(1m / (decimal)rate, 6);
                currency.RateUpdatedAt = now;
                _repo.Update(currency);
                updated++;
            }

            await _repo.SaveChangesAsync(ct);
            _cache.RemoveByPrefix(CacheKeys.Currencies.Prefix);

            _logger.LogInformation(
                "Exchange rates synced for {Count}/{Total} currencies (date: {Date}).",
                updated, nonBase.Count, date);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Exchange rate sync failed: HTTP error contacting provider.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exchange rate sync failed.");
            throw;
        }
    }
}
