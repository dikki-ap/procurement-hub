using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.MasterData.Application.Commands.CreateMaterial;
using ProcureHub.Modules.MasterData.Application.Commands.DeleteMaterial;
using ProcureHub.Modules.MasterData.Application.Commands.UpdateMaterial;
using ProcureHub.Modules.MasterData.Application.Queries.GetMaterialById;
using ProcureHub.Modules.MasterData.Application.Queries.GetMaterialList;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.MasterData;

/// <summary>Material master data — readable by all internal users, writable by super_admin only.</summary>
[ApiController]
[Route("api/v1/master-data/materials")]
[Authorize(Policy = "RequireInternal")]
public class MaterialsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MaterialsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get all materials.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetList(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMaterialListQuery(), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get material by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMaterialByIdQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Create a new material.</summary>
    [HttpPost]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Create(
        [FromBody] CreateMaterialCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse.Ok(new { id }));
    }

    /// <summary>Update a material.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        Guid id, [FromBody] UpdateMaterialCommand command, CancellationToken ct)
    {
        await _mediator.Send(command with { Id = id }, ct);
        return Ok(ApiResponse.Ok("Material updated."));
    }

    /// <summary>Delete a material (soft delete).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteMaterialCommand(id), ct);
        return Ok(ApiResponse.Ok("Material deleted."));
    }
}
