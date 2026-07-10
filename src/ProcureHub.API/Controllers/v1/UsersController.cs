using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Common;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.API.Controllers.v1;

[ApiController]
[Route("api/v1/users")]
[Authorize(Policy = "RequireInternal")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService  _currentUser;

    public UsersController(ApplicationDbContext db, ICurrentUserService currentUser)
    {
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
            id        = user.Id,
            companyId = user.CompanyId,
            email     = user.Email,
            fullName  = user.FullName,
            role      = user.Role,
        }));
    }
}
