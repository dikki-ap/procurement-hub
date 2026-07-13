using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.MasterData.Application.Commands.CreateCurrency;
using ProcureHub.Modules.MasterData.Application.Commands.DeleteCurrency;
using ProcureHub.Modules.MasterData.Application.Commands.UpdateCurrency;
using ProcureHub.Modules.MasterData.Application.Queries.GetCurrencyById;
using ProcureHub.Modules.MasterData.Application.Queries.GetCurrencyList;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.MasterData;

/// <summary>Currency master data — readable by all internal users, writable by super_admin only.</summary>
[ApiController]
[Route("api/v1/master-data/currencies")]
[Authorize(Policy = "RequireInternal")]
public class CurrenciesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CurrenciesController(IMediator mediator) => _mediator = mediator;

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
}
