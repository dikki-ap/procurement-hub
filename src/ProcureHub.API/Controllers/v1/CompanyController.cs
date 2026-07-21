using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.CompanyManagement.Application.Commands.UpdateCompany;
using ProcureHub.Modules.CompanyManagement.Application.Commands.UploadCompanyLogo;
using ProcureHub.Modules.CompanyManagement.Application.Queries.GetCompany;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Common;
using ProcureHub.SharedKernel.Database;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.API.Controllers.v1;

[ApiController]
[Route("api/v1/company")]
[Authorize(Policy = "RequireSuperAdmin")]
public class CompanyController : ControllerBase
{
    private static readonly HashSet<string> AllowedImageTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
    ];

    private readonly IMediator            _mediator;
    private readonly ICurrentUserService  _currentUser;
    private readonly ApplicationDbContext _db;

    public CompanyController(
        IMediator mediator,
        ICurrentUserService currentUser,
        ApplicationDbContext db)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
        _db          = db;
    }

    /// <summary>Get own company profile.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> Get(CancellationToken ct)
    {
        var companyId = await ResolveCallerCompanyIdAsync(ct);
        var result    = await _mediator.Send(new GetCompanyQuery(companyId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Update company profile (name, type, address, phone, email).</summary>
    [HttpPut]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        [FromBody] UpdateCompanyRequest request, CancellationToken ct)
    {
        var companyId = await ResolveCallerCompanyIdAsync(ct);
        await _mediator.Send(new UpdateCompanyCommand(
            companyId, request.Name, request.Type,
            request.Address, request.Phone, request.Email), ct);
        return Ok(ApiResponse.Ok("Company profile updated."));
    }

    /// <summary>Upload company logo (JPEG/PNG/WebP, max 2 MB).</summary>
    [HttpPost("logo")]
    public async Task<ActionResult<ApiResponse<object>>> UploadLogo(
        [FromForm] IFormFile file, CancellationToken ct)
    {
        if (!AllowedImageTypes.Contains(file.ContentType))
            throw new BusinessRuleException("UploadLogo", "Allowed formats: JPEG, PNG, WebP.");

        const long maxBytes = 2L * 1024 * 1024;
        if (file.Length > maxBytes)
            throw new BusinessRuleException("UploadLogo", "Logo must not exceed 2 MB.");

        var companyId = await ResolveCallerCompanyIdAsync(ct);

        using var ms = new MemoryStream((int)file.Length);
        await file.CopyToAsync(ms, ct);
        ms.Position = 0;

        var objectKey = await _mediator.Send(
            new UploadCompanyLogoCommand(companyId, ms, file.FileName, file.ContentType), ct);

        return Ok(ApiResponse.Ok(new { logoKey = objectKey }));
    }

    private async Task<Guid> ResolveCallerCompanyIdAsync(CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException();

        var companyId = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => (Guid?)u.CompanyId)
            .FirstOrDefaultAsync(ct);

        return companyId ?? throw new ForbiddenException();
    }
}

public record UpdateCompanyRequest(
    string  Name,
    string  Type,
    string? Address,
    string? Phone,
    string? Email);
