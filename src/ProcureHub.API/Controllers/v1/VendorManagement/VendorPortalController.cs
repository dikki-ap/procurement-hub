using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.API.Security;
using ProcureHub.Modules.MasterData.Application.Queries.GetDocumentTypeList;
using ProcureHub.Modules.Procurement.Application.Commands.AcknowledgeReturnOrder;
using ProcureHub.Modules.Procurement.Application.Queries.GetContractsByVendor;
using ProcureHub.Modules.Procurement.Application.Queries.GetReturnOrdersByVendor;
using ProcureHub.Modules.VendorManagement.Application.Commands.AddVendorBankAccount;
using ProcureHub.Modules.VendorManagement.Application.Commands.DeleteVendorBankAccount;
using ProcureHub.Modules.VendorManagement.Application.Commands.DeleteVendorDocument;
using ProcureHub.Modules.VendorManagement.Application.Commands.UpdateVendorBankAccount;
using ProcureHub.Modules.VendorManagement.Application.Commands.UpdateVendorProfile;
using ProcureHub.Modules.VendorManagement.Application.Commands.UploadVendorDocument;
using ProcureHub.Modules.VendorManagement.Application.Queries.GetMyVendorId;
using ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorById;
using ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorScoreHistory;
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

    private readonly IMediator                _mediator;
    private readonly ICurrentUserService      _currentUser;
    private readonly IDocumentAccessLogger    _accessLogger;

    public VendorPortalController(IMediator mediator, ICurrentUserService currentUser, IDocumentAccessLogger accessLogger)
    {
        _mediator     = mediator;
        _currentUser  = currentUser;
        _accessLogger = accessLogger;
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

    /// <summary>Update own vendor profile (trade name, legal numbers, address).</summary>
    [HttpPut("{vendorId:guid}/profile")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateProfile(
        Guid vendorId, [FromBody] UpdateVendorProfileRequest request, CancellationToken ct)
    {
        await VerifyOwnershipAsync(vendorId, ct);
        await _mediator.Send(new UpdateVendorProfileCommand(
            vendorId,
            request.TradeName,
            request.Npwp,
            request.Siup,
            request.Nib,
            request.Address,
            request.City,
            request.Province,
            request.PostalCode,
            request.Country), ct);
        return Ok(ApiResponse.Ok("Profile updated."));
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

        // Buffer to MemoryStream first — IFormFile.OpenReadStream() may return a non-seekable
        // RangeReadStream; MemoryStream is always seekable so the signature validator can
        // reset position to 0 before passing the full content to the upload command.
        using var ms = new MemoryStream((int)request.File.Length);
        await request.File.CopyToAsync(ms, ct);
        ms.Position = 0;

        // Verify actual file content matches the declared extension (magic bytes)
        if (!await FileSignatureValidator.IsValidAsync(ms, ext))
            throw new BusinessRuleException("UploadDocument",
                "File content does not match the declared file type.");

        var docId = await _mediator.Send(new UploadVendorDocumentCommand(
            vendorId,
            request.DocumentType,
            request.DocumentNumber,
            ms,
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
        var deletedById = _currentUser.VendorUserId ?? Guid.Empty;
        await _mediator.Send(new DeleteVendorDocumentCommand(documentId, deletedById), ct);
        return Ok(ApiResponse.Ok("Document deleted."));
    }

    /// <summary>Return a short-lived presigned URL for the document. Use ?inline=true for browser preview.</summary>
    [HttpGet("{vendorId:guid}/documents/{documentId:guid}/download")]
    public async Task<ActionResult<ApiResponse<object>>> DownloadDocument(
        Guid vendorId, Guid documentId, [FromQuery] bool inline, CancellationToken ct)
    {
        await VerifyOwnershipAsync(vendorId, ct);
        var result = await _mediator.Send(new GetVendorDocumentDownloadUrlQuery(vendorId, documentId, inline), ct);
        await _accessLogger.LogAsync("VendorDocument", documentId, result.FileName, inline, ct);
        return Ok(ApiResponse.Ok(new { url = result.Url, fileName = result.FileName }));
    }

    /// <summary>Get score history for own vendor profile.</summary>
    [HttpGet("{vendorId:guid}/scores")]
    public async Task<ActionResult<ApiResponse<object>>> GetScoreHistory(Guid vendorId, CancellationToken ct)
    {
        await VerifyOwnershipAsync(vendorId, ct);
        var result = await _mediator.Send(new GetVendorScoreHistoryQuery(vendorId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get active document types available for upload.</summary>
    [HttpGet("{vendorId:guid}/document-types")]
    public async Task<ActionResult<ApiResponse<object>>> GetDocumentTypes(Guid vendorId, CancellationToken ct)
    {
        await VerifyOwnershipAsync(vendorId, ct);
        var result = await _mediator.Send(new GetDocumentTypeListQuery(), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Add a bank account for own vendor profile.</summary>
    [HttpPost("{vendorId:guid}/bank-accounts")]
    public async Task<ActionResult<ApiResponse<object>>> AddBankAccount(
        Guid vendorId, [FromBody] PortalBankAccountRequest request, CancellationToken ct)
    {
        await VerifyOwnershipAsync(vendorId, ct);
        var bankId = await _mediator.Send(new AddVendorBankAccountCommand(
            vendorId, request.BankName, request.AccountNumber, request.AccountName,
            request.BranchName, request.Currency, request.IsDefault, request.Notes), ct);
        return CreatedAtAction(nameof(GetProfile), new { vendorId }, ApiResponse.Ok(new { id = bankId }));
    }

    /// <summary>Update own vendor bank account.</summary>
    [HttpPut("{vendorId:guid}/bank-accounts/{bankAccountId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateBankAccount(
        Guid vendorId, Guid bankAccountId, [FromBody] PortalBankAccountRequest request, CancellationToken ct)
    {
        await VerifyOwnershipAsync(vendorId, ct);
        await _mediator.Send(new UpdateVendorBankAccountCommand(
            bankAccountId, vendorId, request.BankName, request.AccountNumber, request.AccountName,
            request.BranchName, request.Currency, request.IsDefault, request.Notes), ct);
        return Ok(ApiResponse.Ok("Bank account updated."));
    }

    /// <summary>Delete own vendor bank account.</summary>
    [HttpDelete("{vendorId:guid}/bank-accounts/{bankAccountId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteBankAccount(
        Guid vendorId, Guid bankAccountId, CancellationToken ct)
    {
        await VerifyOwnershipAsync(vendorId, ct);
        await _mediator.Send(new DeleteVendorBankAccountCommand(bankAccountId, vendorId), ct);
        return Ok(ApiResponse.Ok("Bank account removed."));
    }

    // ── Contracts (read-only) ─────────────────────────────────────────────────

    /// <summary>Get own contracts (read-only).</summary>
    [HttpGet("{vendorId:guid}/contracts")]
    public async Task<ActionResult<ApiResponse<object>>> GetContracts(Guid vendorId, CancellationToken ct)
    {
        await VerifyOwnershipAsync(vendorId, ct);
        var result = await _mediator.Send(new GetContractsByVendorQuery(vendorId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    // ── Return Orders (read-only + acknowledge) ───────────────────────────────

    /// <summary>Get own return orders.</summary>
    [HttpGet("{vendorId:guid}/return-orders")]
    public async Task<ActionResult<ApiResponse<object>>> GetReturnOrders(Guid vendorId, CancellationToken ct)
    {
        await VerifyOwnershipAsync(vendorId, ct);
        var result = await _mediator.Send(new GetReturnOrdersByVendorQuery(vendorId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Acknowledge a return order (vendor confirms they will ship it back).</summary>
    [HttpPost("{vendorId:guid}/return-orders/{returnOrderId:guid}/acknowledge")]
    public async Task<ActionResult<ApiResponse<object>>> AcknowledgeReturn(
        Guid vendorId, Guid returnOrderId, CancellationToken ct)
    {
        await VerifyOwnershipAsync(vendorId, ct);
        await _mediator.Send(new AcknowledgeReturnOrderCommand(returnOrderId, vendorId), ct);
        return Ok(ApiResponse.Ok("Return order acknowledged."));
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

public record PortalBankAccountRequest(
    string  BankName,
    string  AccountNumber,
    string  AccountName,
    string? BranchName,
    string  Currency,
    bool    IsDefault,
    string? Notes);

public record UpdateVendorProfileRequest(
    string? TradeName,
    string? Npwp,
    string? Siup,
    string? Nib,
    string? Address,
    string? City,
    string? Province,
    string? PostalCode,
    string? Country);
