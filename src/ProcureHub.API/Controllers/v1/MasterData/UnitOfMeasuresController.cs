using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.MasterData.Application.Commands.CreateUnitOfMeasure;
using ProcureHub.Modules.MasterData.Application.Commands.DeleteUnitOfMeasure;
using ProcureHub.Modules.MasterData.Application.Commands.UpdateUnitOfMeasure;
using ProcureHub.Modules.MasterData.Application.Queries.GetUnitOfMeasureById;
using ProcureHub.Modules.MasterData.Application.Queries.GetUnitOfMeasureList;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.MasterData;

/// <summary>Unit of Measure master data management.</summary>
[ApiController]
[Route("api/v1/master-data/unit-of-measures")]
[Authorize(Policy = "RequireMasterData")]
public class UnitOfMeasuresController : ControllerBase
{
    private readonly IMediator _mediator;

    public UnitOfMeasuresController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get all units of measure for a company.</summary>
    [HttpGet]
    [Authorize(Policy = "RequireInternal")]
    public async Task<ActionResult<ApiResponse<object>>> GetList(
        [FromQuery] Guid companyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUnitOfMeasureListQuery(companyId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get unit of measure by ID.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "RequireInternal")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUnitOfMeasureByIdQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Create a new unit of measure.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> Create(
        [FromBody] CreateUnitOfMeasureCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse.Ok(new { id }));
    }

    /// <summary>Update a unit of measure.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        Guid id, [FromBody] UpdateUnitOfMeasureCommand command, CancellationToken ct)
    {
        await _mediator.Send(command with { Id = id }, ct);
        return Ok(ApiResponse.Ok("Unit of measure updated."));
    }

    /// <summary>Delete a unit of measure.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteUnitOfMeasureCommand(id), ct);
        return Ok(ApiResponse.Ok("Unit of measure deleted."));
    }
}
