using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.MasterData.Application.Commands.CreateMaterialCategory;
using ProcureHub.Modules.MasterData.Application.Commands.DeleteMaterialCategory;
using ProcureHub.Modules.MasterData.Application.Commands.UpdateMaterialCategory;
using ProcureHub.Modules.MasterData.Application.Queries.GetMaterialCategoryById;
using ProcureHub.Modules.MasterData.Application.Queries.GetMaterialCategoryList;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.MasterData;

/// <summary>Material Category master data management.</summary>
[ApiController]
[Route("api/v1/master-data/material-categories")]
[Authorize(Policy = "RequireMasterData")]
public class MaterialCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MaterialCategoriesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get all material categories for a company.</summary>
    [HttpGet]
    [Authorize(Policy = "RequireInternal")]
    public async Task<ActionResult<ApiResponse<object>>> GetList(
        [FromQuery] Guid companyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMaterialCategoryListQuery(companyId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get material category by ID.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "RequireInternal")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMaterialCategoryByIdQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Create a new material category.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> Create(
        [FromBody] CreateMaterialCategoryCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse.Ok(new { id }));
    }

    /// <summary>Update a material category.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        Guid id, [FromBody] UpdateMaterialCategoryCommand command, CancellationToken ct)
    {
        await _mediator.Send(command with { Id = id }, ct);
        return Ok(ApiResponse.Ok("Material category updated."));
    }

    /// <summary>Delete a material category.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteMaterialCategoryCommand(id), ct);
        return Ok(ApiResponse.Ok("Material category deleted."));
    }
}
