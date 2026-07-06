using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.ApprovalEngine.Application.Commands.CreateApprovalPolicy;
using ProcureHub.Modules.ApprovalEngine.Application.Queries.GetApprovalPolicies;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.Approval;

/// <summary>Approval policy configuration — defines how many levels are required per value range.</summary>
[ApiController]
[Route("api/v1/approval-policies")]
[Authorize(Policy = "RequireSuperAdmin")]
public class ApprovalPoliciesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApprovalPoliciesController(IMediator mediator) => _mediator = mediator;

    /// <summary>List all approval policies for a company.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetList(
        [FromQuery] Guid companyId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetApprovalPoliciesQuery(companyId), ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Create a new approval policy for a company.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> Create(
        [FromBody] CreateApprovalPolicyCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return Ok(ApiResponse.Ok(id));
    }
}
