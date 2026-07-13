using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.MasterData.Application.Commands.CreateCurrency;
using ProcureHub.Modules.MasterData.Application.Commands.DeleteCurrency;
using ProcureHub.Modules.MasterData.Application.Commands.UpdateCurrency;
using ProcureHub.Modules.MasterData.Application.Queries.GetCurrencyById;
using ProcureHub.Modules.MasterData.Application.Queries.GetCurrencyList;
using ProcureHub.Modules.MasterData.Application.Services;
using ProcureHub.Modules.MasterData.Infrastructure.Jobs;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.MasterData;

/// <summary>Currency master data — readable by all internal users, writable by super_admin only.</summary>
[ApiController]
[Route("api/v1/master-data/currencies")]
[Authorize(Policy = "RequireInternal")]
public class CurrenciesController : ControllerBase
{
    private readonly IMediator                  _mediator;
    private readonly IExchangeRateService       _exchangeRateService;
    private readonly IExchangeRateConfigService _configService;

    public CurrenciesController(
        IMediator                  mediator,
        IExchangeRateService       exchangeRateService,
        IExchangeRateConfigService configService)
    {
        _mediator            = mediator;
        _exchangeRateService = exchangeRateService;
        _configService       = configService;
    }

    /// <summary>Get all currencies.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetList(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCurrencyListQuery(), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get currency by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCurrencyByIdQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Create a new currency.</summary>
    [HttpPost]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Create(
        [FromBody] CreateCurrencyCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse.Ok(new { id }));
    }

    /// <summary>Update a currency.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        Guid id, [FromBody] UpdateCurrencyCommand command, CancellationToken ct)
    {
        await _mediator.Send(command with { Id = id }, ct);
        return Ok(ApiResponse.Ok("Currency updated."));
    }

    /// <summary>Delete a currency.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteCurrencyCommand(id), ct);
        return Ok(ApiResponse.Ok("Currency deleted."));
    }

    /// <summary>Manually trigger exchange rate sync.</summary>
    [HttpPost("fetch-rates")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> FetchRates(CancellationToken ct)
    {
        await _exchangeRateService.SyncRatesAsync(ct);
        return Ok(ApiResponse.Ok("Exchange rates updated successfully."));
    }

    /// <summary>Get exchange rate sync settings (auto vs manual).</summary>
    [HttpGet("exchange-rate-settings")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> GetExchangeRateSettings(CancellationToken ct)
    {
        var autoSync = await _configService.GetAutoSyncAsync(ct);
        return Ok(ApiResponse.Ok(new { autoSync }));
    }

    /// <summary>Update exchange rate sync mode. Enabling auto-sync registers the daily Hangfire job; disabling removes it.</summary>
    [HttpPut("exchange-rate-settings")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateExchangeRateSettings(
        [FromBody] UpdateExchangeRateSettingsRequest request, CancellationToken ct)
    {
        await _configService.SetAutoSyncAsync(request.AutoSync, ct);

        if (request.AutoSync)
            RecurringJob.AddOrUpdate<ExchangeRateJob>(
                "exchange-rate-sync",
                job => job.ExecuteAsync(CancellationToken.None),
                "5 9 * * *",
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
        else
            RecurringJob.RemoveIfExists("exchange-rate-sync");

        return Ok(ApiResponse.Ok("Exchange rate settings updated."));
    }
}

public record UpdateExchangeRateSettingsRequest(bool AutoSync);
