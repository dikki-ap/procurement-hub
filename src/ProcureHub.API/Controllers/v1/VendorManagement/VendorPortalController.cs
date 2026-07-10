using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.VendorManagement.Application.Commands.DeleteVendorDocument;
using ProcureHub.Modules.VendorManagement.Application.Commands.UploadVendorDocument;
using ProcureHub.Modules.VendorManagement.Application.Queries.GetMyVendorId;
using ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorById;
using ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorDocuments;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.VendorManagement;

/// <summary>Vendor self-service portal — vendor users only.</summary>
[ApiController]
[Route("api/v1/vendor-portal")]
[Authorize(Policy = "RequireVendorUser")]
public class VendorPortalController : ControllerBase
{
    private readonly IMediator           _mediator;
    private readonly ICurrentUserService _currentUser;

    public VendorPortalController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
    }

    /// <summary>Resolve the vendorId for the currently authenticated vendor user.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<object>>> GetMyVendorId(CancellationToken ct)
    {
        var keycloakId = _currentUser.KeycloakId;
        if (string.IsNullOrEmpty(keycloakId))
            return Unauthorized();

        var vendorId = await _mediator.Send(new GetMyVendorIdQuery(keycloakId), ct);
        return Ok(ApiResponse.Ok(new { vendorId }));
    }

    /// <summary>Get own vendor profile.</summary>
    [HttpGet("{vendorId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetProfile(Guid vendorId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVendorByIdQuery(vendorId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get own vendor documents.</summary>
    [HttpGet("{vendorId:guid}/documents")]
    public async Task<ActionResult<ApiResponse<object>>> GetDocuments(Guid vendorId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVendorDocumentsQuery(vendorId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Upload a document for own vendor profile.</summary>
    [HttpPost("{vendorId:guid}/documents")]
    public async Task<ActionResult<ApiResponse<object>>> UploadDocument(
        Guid vendorId, [FromForm] UploadDocumentRequest request, CancellationToken ct)
    {
        await using var stream = request.File.OpenReadStream();
        var docId = await _mediator.Send(new UploadVendorDocumentCommand(
            vendorId,
            request.DocumentType,
            request.DocumentNumber,
            stream,
            request.File.FileName,
            request.File.ContentType,
            request.ExpiredAt,
            request.IssuedAt,
            request.Notes), ct);

        return CreatedAtAction(nameof(GetDocuments), new { vendorId }, ApiResponse.Ok(new { id = docId }));
    }

    /// <summary>Delete own vendor document.</summary>
    [HttpDelete("{vendorId:guid}/documents/{documentId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDocument(
        Guid vendorId, Guid documentId, CancellationToken ct)
    {
        var deletedById = _currentUser.UserId ?? Guid.Empty;
        await _mediator.Send(new DeleteVendorDocumentCommand(documentId, deletedById), ct);
        return Ok(ApiResponse.Ok("Document deleted."));
    }
}
