using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.Procurement.Application.Commands.ActivateContract;
using ProcureHub.Modules.Procurement.Application.Commands.CreateContract;
using ProcureHub.Modules.Procurement.Application.Commands.TerminateContract;
using ProcureHub.Modules.Procurement.Application.Commands.UpdateContract;
using ProcureHub.Modules.Procurement.Application.Commands.UploadContractFile;
using ProcureHub.Modules.Procurement.Application.Queries.GetContractById;
using ProcureHub.Modules.Procurement.Application.Queries.GetContractDownloadUrl;
using ProcureHub.Modules.Procurement.Application.Queries.GetContractList;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.Procurement;

/// <summary>Contract management.</summary>
[ApiController]
[Route("api/v1/contracts")]
[Authorize(Policy = "RequireInternal")]
public class ContractsController : ControllerBase
{
    private static readonly HashSet<string> AllowedContentTypes =
    [
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "image/jpeg",
        "image/png",
    ];

    private const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 MB

    private readonly IMediator           _mediator;
    private readonly ICurrentUserService _currentUser;

    public ContractsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
    }

    /// <summary>List contracts for the caller's company.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetList(CancellationToken ct)
    {
        var companyId = await ResolveCompanyIdAsync(ct);
        var result    = await _mediator.Send(new GetContractListQuery(companyId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get contract detail by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContractByIdQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Create a new contract (draft).</summary>
    [HttpPost]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> Create(
        [FromBody] CreateContractRequest body, CancellationToken ct)
    {
        var companyId = await ResolveCompanyIdAsync(ct);
        var id = await _mediator.Send(new CreateContractCommand(
            companyId,
            body.VendorId,
            body.Title,
            body.POId,
            body.StartDate,
            body.EndDate,
            body.Value,
            body.CurrencyId,
            body.Notes), ct);

        return CreatedAtAction(nameof(GetById), new { id }, ApiResponse.Ok(id));
    }

    /// <summary>Update a draft contract.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        Guid id, [FromBody] UpdateContractRequest body, CancellationToken ct)
    {
        await _mediator.Send(new UpdateContractCommand(
            id,
            body.Title,
            body.POId,
            body.StartDate,
            body.EndDate,
            body.Value,
            body.CurrencyId,
            body.Notes), ct);

        return Ok(ApiResponse.Ok("Contract updated."));
    }

    /// <summary>Activate a draft contract.</summary>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> Activate(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new ActivateContractCommand(id), ct);
        return Ok(ApiResponse.Ok("Contract activated."));
    }

    /// <summary>Terminate an active contract.</summary>
    [HttpPost("{id:guid}/terminate")]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> Terminate(
        Guid id, [FromBody] TerminateContractRequest body, CancellationToken ct)
    {
        await _mediator.Send(new TerminateContractCommand(id, body.Reason), ct);
        return Ok(ApiResponse.Ok("Contract terminated."));
    }

    /// <summary>Upload a signed contract document (PDF/DOCX, max 20 MB).</summary>
    [HttpPost("{id:guid}/upload")]
    [Authorize(Policy = "RequirePurchasing")]
    public async Task<ActionResult<ApiResponse<object>>> Upload(
        Guid id, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Fail<object>("No file provided."));

        if (!AllowedContentTypes.Contains(file.ContentType))
            return BadRequest(ApiResponse.Fail<object>("Only PDF, DOCX, JPEG, PNG files are accepted."));

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(ApiResponse.Fail<object>("File size must not exceed 20 MB."));

        using var stream = file.OpenReadStream();
        var key = await _mediator.Send(
            new UploadContractFileCommand(id, stream, file.FileName, file.ContentType), ct);

        return Ok(ApiResponse.Ok(new { key }));
    }

    /// <summary>Get a presigned download URL for the contract document.</summary>
    [HttpGet("{id:guid}/download")]
    public async Task<ActionResult<ApiResponse<object>>> Download(Guid id, CancellationToken ct)
    {
        var url = await _mediator.Send(new GetContractDownloadUrlQuery(id), ct);
        return Ok(ApiResponse.Ok(new { url }));
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private async Task<Guid> ResolveCompanyIdAsync(CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            throw new UnauthorizedAccessException("User identity not resolved.");

        var db = HttpContext.RequestServices.GetRequiredService<ProcureHub.SharedKernel.Database.ApplicationDbContext>();
        var user = await db.Users.FindAsync([userId.Value], ct)
            ?? throw new ProcureHub.SharedKernel.Exceptions.NotFoundException("User", userId.Value);
        return user.CompanyId;
    }
}

// ── Request bodies ───────────────────────────────────────────────────────────

public record CreateContractRequest(
    Guid      VendorId,
    string    Title,
    Guid?     POId,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal?  Value,
    Guid?     CurrencyId,
    string?   Notes);

public record UpdateContractRequest(
    string    Title,
    Guid?     POId,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal?  Value,
    Guid?     CurrencyId,
    string?   Notes);

public record TerminateContractRequest(string? Reason);
