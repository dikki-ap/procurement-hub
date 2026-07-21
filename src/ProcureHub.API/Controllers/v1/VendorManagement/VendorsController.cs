using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.API.Security;
using ProcureHub.Modules.MasterData.Application.Queries.GetDocumentTypeList;
using ProcureHub.Modules.VendorManagement.Application.Commands.AddVendorCapability;
using ProcureHub.Modules.VendorManagement.Application.Commands.ApproveVendor;
using ProcureHub.Modules.VendorManagement.Application.Commands.BlacklistVendor;
using ProcureHub.Modules.VendorManagement.Application.Commands.DeleteVendorCapability;
using ProcureHub.Modules.VendorManagement.Application.Commands.DeleteVendorDocument;
using ProcureHub.Modules.VendorManagement.Application.Commands.ReinstateVendor;
using ProcureHub.Modules.VendorManagement.Application.Commands.SuspendVendor;
using ProcureHub.Modules.VendorManagement.Application.Commands.UpdateVendorCapability;
using ProcureHub.Modules.VendorManagement.Application.Commands.UploadVendorDocument;
using ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorById;
using ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorDocumentDownloadUrl;
using ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorDocuments;
using ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorList;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Common;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.API.Controllers.v1.VendorManagement;

/// <summary>Admin vendor management — internal users only.</summary>
[ApiController]
[Route("api/v1/vendors")]
[Authorize(Policy = "RequireInternal")]
public class VendorsController : ControllerBase
{
    private readonly IMediator           _mediator;
    private readonly ICurrentUserService _currentUser;

    public VendorsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
    }

    /// <summary>List all vendors for a company.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetList(
        [FromQuery] Guid companyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVendorListQuery(companyId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get vendor detail by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVendorByIdQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get all documents for a vendor.</summary>
    [HttpGet("{id:guid}/documents")]
    public async Task<ActionResult<ApiResponse<object>>> GetDocuments(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVendorDocumentsQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Approve a pending vendor.</summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Approve(Guid id, CancellationToken ct)
    {
        var approvedById = _currentUser.UserId ?? Guid.Empty;
        await _mediator.Send(new ApproveVendorCommand(id, approvedById), ct);
        return Ok(ApiResponse.Ok("Vendor approved."));
    }

    /// <summary>Suspend an active vendor.</summary>
    [HttpPost("{id:guid}/suspend")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Suspend(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new SuspendVendorCommand(id), ct);
        return Ok(ApiResponse.Ok("Vendor suspended."));
    }

    /// <summary>Blacklist a vendor.</summary>
    [HttpPost("{id:guid}/blacklist")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Blacklist(
        Guid id, [FromBody] BlacklistRequest request, CancellationToken ct)
    {
        var blacklistedById = _currentUser.UserId ?? Guid.Empty;
        await _mediator.Send(new BlacklistVendorCommand(id, request.Reason, blacklistedById), ct);
        return Ok(ApiResponse.Ok("Vendor blacklisted."));
    }

    /// <summary>Reinstate a suspended or blacklisted vendor.</summary>
    [HttpPost("{id:guid}/reinstate")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Reinstate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ReinstateVendorCommand(id), ct);
        return Ok(ApiResponse.Ok("Vendor reinstated."));
    }

    private static readonly HashSet<string> GlobalAllowedTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-excel",
    ];

    /// <summary>Upload a document for a vendor.</summary>
    [HttpPost("{id:guid}/documents")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> UploadDocument(
        Guid id, [FromForm] UploadDocumentRequest request, CancellationToken ct)
    {
        var docTypes = await _mediator.Send(new GetDocumentTypeListQuery(), ct);
        var docType  = docTypes.FirstOrDefault(t => t.Name == request.DocumentType && t.IsActive)
            ?? throw new BusinessRuleException("UploadDocument",
                $"Document type '{request.DocumentType}' is not recognised or is inactive.");

        var ext      = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        var maxBytes = (long)docType.MaxFileSizeMb * 1024 * 1024;

        if (!GlobalAllowedTypes.Contains(request.File.ContentType))
            throw new BusinessRuleException("UploadDocument", "Allowed content types: PDF, JPEG, PNG, XLSX.");

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

        using var ms = new MemoryStream((int)request.File.Length);
        await request.File.CopyToAsync(ms, ct);
        ms.Position = 0;

        if (!await FileSignatureValidator.IsValidAsync(ms, ext))
            throw new BusinessRuleException("UploadDocument",
                "File content does not match the declared file type.");

        var docId = await _mediator.Send(new UploadVendorDocumentCommand(
            id,
            request.DocumentType,
            request.DocumentNumber,
            ms,
            request.File.FileName,
            request.File.ContentType,
            docType.MaxFileSizeMb,
            request.ExpiredAt,
            request.IssuedAt,
            request.Notes), ct);

        return CreatedAtAction(nameof(GetDocuments), new { id }, ApiResponse.Ok(new { id = docId }));
    }

    /// <summary>Delete a vendor document.</summary>
    [HttpDelete("{id:guid}/documents/{documentId:guid}")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteDocument(
        Guid id, Guid documentId, CancellationToken ct)
    {
        var deletedById = _currentUser.UserId ?? Guid.Empty;
        await _mediator.Send(new DeleteVendorDocumentCommand(documentId, deletedById), ct);
        return Ok(ApiResponse.Ok("Document deleted."));
    }

    /// <summary>Return a short-lived presigned URL for the document. Use ?inline=true for browser preview.</summary>
    [HttpGet("{id:guid}/documents/{documentId:guid}/download")]
    public async Task<ActionResult<ApiResponse<object>>> DownloadDocument(
        Guid id, Guid documentId, [FromQuery] bool inline, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVendorDocumentDownloadUrlQuery(id, documentId, inline), ct);
        return Ok(ApiResponse.Ok(new { url = result.Url, fileName = result.FileName }));
    }

    /// <summary>Add a capability (approved supply category) to a vendor.</summary>
    [HttpPost("{id:guid}/capabilities")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> AddCapability(
        Guid id, [FromBody] AddCapabilityRequest request, CancellationToken ct)
    {
        var capId = await _mediator.Send(new AddVendorCapabilityCommand(
            id, request.MaterialCategoryId, request.MinOrderQty, request.LeadTimeDays, request.Notes), ct);
        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse.Ok(new { id = capId }));
    }

    /// <summary>Update a vendor capability's operational details.</summary>
    [HttpPut("{id:guid}/capabilities/{capabilityId:guid}")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateCapability(
        Guid id, Guid capabilityId, [FromBody] UpdateCapabilityRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateVendorCapabilityCommand(
            capabilityId, request.MinOrderQty, request.LeadTimeDays, request.Notes), ct);
        return Ok(ApiResponse.Ok("Capability updated."));
    }

    /// <summary>Remove a capability from a vendor.</summary>
    [HttpDelete("{id:guid}/capabilities/{capabilityId:guid}")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCapability(
        Guid id, Guid capabilityId, CancellationToken ct)
    {
        await _mediator.Send(new DeleteVendorCapabilityCommand(capabilityId), ct);
        return Ok(ApiResponse.Ok("Capability removed."));
    }
}

public record BlacklistRequest(string Reason);

public record AddCapabilityRequest(
    Guid     MaterialCategoryId,
    decimal? MinOrderQty,
    int?     LeadTimeDays,
    string?  Notes);

public record UpdateCapabilityRequest(
    decimal? MinOrderQty,
    int?     LeadTimeDays,
    string?  Notes);

public class UploadDocumentRequest
{
    public IFormFile File           { get; set; } = null!;
    public string    DocumentType   { get; set; } = string.Empty;
    public string?   DocumentNumber { get; set; }
    public DateOnly? ExpiredAt      { get; set; }
    public DateOnly? IssuedAt       { get; set; }
    public string?   Notes          { get; set; }
}
