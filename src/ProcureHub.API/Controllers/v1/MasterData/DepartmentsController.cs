using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.MasterData.Application.Commands.CreateDepartment;
using ProcureHub.Modules.MasterData.Application.Commands.DeleteDepartment;
using ProcureHub.Modules.MasterData.Application.Commands.UpdateDepartment;
using ProcureHub.Modules.MasterData.Application.Queries.GetDepartmentList;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Common;
using ProcureHub.SharedKernel.Database;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.API.Controllers.v1.MasterData;

[ApiController]
[Route("api/v1/master-data/departments")]
[Authorize(Policy = "RequireInternal")]
public class DepartmentsController : ControllerBase
{
    private readonly IMediator            _mediator;
    private readonly ICurrentUserService  _currentUser;
    private readonly ApplicationDbContext _db;

    public DepartmentsController(
        IMediator mediator,
        ICurrentUserService currentUser,
        ApplicationDbContext db)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
        _db          = db;
    }

    /// <summary>List all departments for a company.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetList(
        [FromQuery] Guid companyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDepartmentListQuery(companyId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Create a new department.</summary>
    [HttpPost]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Create(
        [FromBody] DepartmentRequest request, CancellationToken ct)
    {
        var companyId = await ResolveCallerCompanyIdAsync(ct);
        var id = await _mediator.Send(
            new CreateDepartmentCommand(companyId, request.Name, request.Code, request.IsActive ?? true), ct);
        return CreatedAtAction(nameof(GetList), new { companyId }, ApiResponse.Ok(new { id }));
    }

    /// <summary>Update an existing department.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        Guid id, [FromBody] DepartmentRequest request, CancellationToken ct)
    {
        await _mediator.Send(
            new UpdateDepartmentCommand(id, request.Name, request.Code, request.IsActive ?? true), ct);
        return Ok(ApiResponse.Ok("Department updated."));
    }

    /// <summary>Delete a department (fails if users are assigned to it).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireMasterData")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteDepartmentCommand(id), ct);
        return Ok(ApiResponse.Ok("Department deleted."));
    }

    private async Task<Guid> ResolveCallerCompanyIdAsync(CancellationToken ct)
    {
        var userId = _currentUser.UserId
            ?? throw new ForbiddenException();

        var companyId = await _db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => (Guid?)u.CompanyId)
            .FirstOrDefaultAsync(ct);

        return companyId ?? throw new ForbiddenException();
    }
}

public record DepartmentRequest(string Name, string Code, bool? IsActive);
