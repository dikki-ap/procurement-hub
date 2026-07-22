using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.MasterData.Application.Commands.CreateDocumentType;
using ProcureHub.Modules.MasterData.Application.Commands.DeleteDocumentType;
using ProcureHub.Modules.MasterData.Application.Commands.UpdateDocumentType;
using ProcureHub.Modules.MasterData.Application.Queries.GetDocumentTypeById;
using ProcureHub.Modules.MasterData.Application.Queries.GetDocumentTypeList;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.MasterData;

/// <summary>Document type master data — readable by internal users, writable by super_admin only.</summary>
[ApiController]
[Route("api/v1/master-data/document-types")]
[Authorize(Policy = "RequireInternal")]
public class DocumentTypesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DocumentTypesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Get all document types (global list).</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetList(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDocumentTypeListQuery(), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get document type by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDocumentTypeByIdQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Create a new document type.</summary>
    [HttpPost]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Create(
        [FromBody] CreateDocumentTypeCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse.Ok(new { id }));
    }

    /// <summary>Update a document type.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        Guid id, [FromBody] UpdateDocumentTypeCommand command, CancellationToken ct)
    {
        await _mediator.Send(command with { Id = id }, ct);
        return Ok(ApiResponse.Ok("Document type updated."));
    }

    /// <summary>Delete a document type.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteDocumentTypeCommand(id), ct);
        return Ok(ApiResponse.Ok("Document type deleted."));
    }
}
