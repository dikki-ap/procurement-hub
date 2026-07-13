using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.API.Security;
using ProcureHub.Modules.MasterData.Application.Queries.GetDocumentTypeList;
using ProcureHub.Modules.VendorManagement.Application.Commands.DeleteVendorDocument;
using ProcureHub.Modules.VendorManagement.Application.Commands.UploadVendorDocument;
using ProcureHub.Modules.VendorManagement.Application.Queries.GetMyVendorId;
using ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorById;
using ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorDocumentDownloadUrl;
using ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorDocuments;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Common;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.API.Controllers.v1.VendorManagement;

/// <summary>Vendor self-service portal — vendor users only.</summary>
[ApiController]
[Route("api/v1/vendor-portal")]
[Authorize(Policy = "RequireVendorUser")]
public class VendorPortalController : ControllerBase
{
    private static readonly HashSet<string> GlobalAllowedTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-excel",
    ];

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
        await VerifyOwnershipAsync(vendorId, ct);
        var result = await _mediator.Send(new GetVendorByIdQuery(vendorId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get own vendor documents.</summary>
    [HttpGet("{vendorId:guid}/documents")]
    public async Task<ActionResult<ApiResponse<object>>> GetDocuments(Guid vendorId, CancellationToken ct)
    {
        await VerifyOwnershipAsync(vendorId, ct);
        var result = await _mediator.Send(new GetVendorDocumentsQuery(vendorId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Upload a document for own vendor profile.</summary>
    [HttpPost("{vendorId:guid}/documents")]
    public async Task<ActionResult<ApiResponse<object>>> UploadDocument(
        Guid vendorId, [FromForm] UploadDocumentRequest request, CancellationToken ct)
    {
        await VerifyOwnershipAsync(vendorId, ct);

        // Validate document type exists and enforce per-type constraints
        var docTypes = await _mediator.Send(new GetDocumentTypeListQuery(), ct);
        var docType  = docTypes.FirstOrDefault(t => t.Name == request.DocumentType && t.IsActive)
            ?? throw new BusinessRuleException("UploadDocument",
                $"Document type '{request.DocumentType}' is not recognised or is inactive.");

        var ext      = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        var maxBytes = (long)docType.MaxFileSizeMb * 1024 * 1024;

        if (!GlobalAllowedTypes.Contains(request.File.ContentType))
            throw new BusinessRuleException("UploadDocument",
                "Allowed content types: PDF, JPEG, PNG, XLSX.");

        if (!string.IsNullOrWhiteSpace(docType.AllowedExtensions))
        {
            var allowed = docType.AllowedExtensions
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!allowed.Contains(ext))
                throw new BusinessRuleException("UploadDocument",
                    $"'{ext}' is not allowed for {docType.Name}. Allowed: {docType.AllowedExtensions}.");
        }

        if (request.File.Length > maxBytes)
            throw new BusinessRuleException("UploadDocument",
                $"File exceeds the {docType.MaxFileSizeMb} MB limit for {docType.Name}.");

        await using var stream = request.File.OpenReadStream();

        // Verify actual file content matches the declared extension (magic bytes)
        if (!await FileSignatureValidator.IsValidAsync(stream, ext))
            throw new BusinessRuleException("UploadDocument",
                "File content does not match the declared file type.");

        var docId = await _mediator.Send(new UploadVendorDocumentCommand(
            vendorId,
            request.DocumentType,
            request.DocumentNumber,
            stream,
            request.File.FileName,
            request.File.ContentType,
            docType.MaxFileSizeMb,
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
        await VerifyOwnershipAsync(vendorId, ct);
        var deletedById = _currentUser.UserId ?? Guid.Empty;
        await _mediator.Send(new DeleteVendorDocumentCommand(documentId, deletedById), ct);
        return Ok(ApiResponse.Ok("Document deleted."));
    }

    /// <summary>Get a presigned download URL for own vendor document (valid 15 min).</summary>
    [HttpGet("{vendorId:guid}/documents/{documentId:guid}/download")]
    public async Task<ActionResult<ApiResponse<object>>> GetDocumentDownloadUrl(
        Guid vendorId, Guid documentId, CancellationToken ct)
    {
        await VerifyOwnershipAsync(vendorId, ct);
        var url = await _mediator.Send(new GetVendorDocumentDownloadUrlQuery(vendorId, documentId), ct);
        return Ok(ApiResponse.Ok(new { url }));
    }

    /// <summary>Get active document types available for upload.</summary>
    [HttpGet("{vendorId:guid}/document-types")]
    public async Task<ActionResult<ApiResponse<object>>> GetDocumentTypes(Guid vendorId, CancellationToken ct)
    {
        await VerifyOwnershipAsync(vendorId, ct);
        var result = await _mediator.Send(new GetDocumentTypeListQuery(), ct);
        return Ok(ApiResponse.Ok(result));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task VerifyOwnershipAsync(Guid vendorId, CancellationToken ct)
    {
        var keycloakId = _currentUser.KeycloakId;
        if (string.IsNullOrEmpty(keycloakId))
            throw new ForbiddenException();

        var myVendorId = await _mediator.Send(new GetMyVendorIdQuery(keycloakId), ct);
        if (myVendorId != vendorId)
            throw new ForbiddenException("You do not have access to this vendor.");
    }
}
