using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.UserManagement.Application.Commands.DeactivateUser;
using ProcureHub.Modules.UserManagement.Application.Commands.UpdateUser;
using ProcureHub.Modules.UserManagement.Application.Queries.GetUserById;
using ProcureHub.Modules.UserManagement.Application.Queries.GetUserList;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Common;
using ProcureHub.SharedKernel.Database;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.API.Controllers.v1;

[ApiController]
[Route("api/v1/users")]
[Authorize(Policy = "RequireInternal")]
public class UsersController : ControllerBase
{
    private readonly IMediator            _mediator;
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService  _currentUser;

    public UsersController(IMediator mediator, ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _mediator    = mediator;
        _db          = db;
        _currentUser = currentUser;
    }

    /// <summary>Returns the authenticated internal user's profile including companyId.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<object>>> GetMe(CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue) return Unauthorized();

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId.Value, ct);

        if (user is null) return NotFound();

        return Ok(ApiResponse.Ok(new
        {
            id           = user.Id,
            companyId    = user.CompanyId,
            email        = user.Email,
            fullName     = user.FullName,
            role         = user.Role,
            departmentId = user.DepartmentId,
            isActive     = user.IsActive,
        }));
    }

    /// <summary>List all users for a company (super_admin only).</summary>
    [HttpGet]
    [Authorize(Policy = "RequireSuperAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> GetList(
        [FromQuery] Guid companyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserListQuery(companyId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get a user by ID (super_admin only).</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "RequireSuperAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Update user department, role, and active status (super_admin only).</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireSuperAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> Update(
        Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var callerId = _currentUser.UserId ?? throw new ForbiddenException();

        if (id == callerId)
            throw new BusinessRuleException("UpdateUser", "You cannot modify your own account through this endpoint.");

        await _mediator.Send(new UpdateUserCommand(id, request.DepartmentId, request.Role, request.IsActive), ct);
        return Ok(ApiResponse.Ok("User updated."));
    }

    /// <summary>Deactivate a user (super_admin only). Cannot deactivate own account.</summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "RequireSuperAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id, CancellationToken ct)
    {
        var callerId = _currentUser.UserId ?? throw new ForbiddenException();

        if (id == callerId)
            throw new BusinessRuleException("DeactivateUser", "You cannot deactivate your own account.");

        await _mediator.Send(new DeactivateUserCommand(id), ct);
        return Ok(ApiResponse.Ok("User deactivated."));
    }
}

public record UpdateUserRequest(Guid? DepartmentId, string Role, bool IsActive);
