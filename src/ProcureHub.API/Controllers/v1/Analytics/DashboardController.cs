using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.Analytics.Application.Queries.GetDashboardWidgets;
using ProcureHub.Modules.Analytics.Application.Queries.GetProcurementFunnelStats;
using ProcureHub.Modules.Analytics.Application.Queries.GetSpendSummary;
using ProcureHub.Modules.Analytics.Application.Queries.GetVendorPerformanceSummary;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.Common;

namespace ProcureHub.API.Controllers.v1.Analytics;

/// <summary>Analytics and dashboard data.</summary>
[ApiController]
[Route("api/v1/analytics")]
[Authorize(Policy = "RequireInternal")]
public class DashboardController : ControllerBase
{
    private readonly IMediator           _mediator;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
    }

    /// <summary>Role-appropriate dashboard widgets.</summary>
    [HttpGet("widgets")]
    public async Task<IActionResult> GetWidgets(
        [FromQuery] Guid? companyId,
        [FromQuery] Guid? vendorId,
        CancellationToken ct)
    {
        var role   = _currentUser.Roles.FirstOrDefault() ?? "internal";
        var query  = new GetDashboardWidgetsQuery(
            CompanyId: companyId ?? Guid.Empty,
            Role:      role,
            VendorId:  vendorId);

        var result = await _mediator.Send(query, ct);
        return Ok(ApiResponse<object>.Ok(result));
    }

    /// <summary>Monthly spend summary for the last N months.</summary>
    [HttpGet("spend-summary")]
    public async Task<IActionResult> GetSpendSummary(
        [FromQuery] Guid companyId,
        [FromQuery] int  months = 12,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetSpendSummaryQuery(companyId, months), ct);
        return Ok(ApiResponse<SpendSummaryDto>.Ok(result));
    }

    /// <summary>Top-N vendor performance summary.</summary>
    [HttpGet("vendor-performance")]
    public async Task<IActionResult> GetVendorPerformance(
        [FromQuery] Guid companyId,
        [FromQuery] int  topN = 10,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetVendorPerformanceSummaryQuery(companyId, topN), ct);
        return Ok(ApiResponse<VendorPerformanceSummaryDto>.Ok(result));
    }

    /// <summary>Procurement funnel stage counts for a given year.</summary>
    [HttpGet("funnel")]
    public async Task<IActionResult> GetFunnelStats(
        [FromQuery] Guid companyId,
        [FromQuery] int  year = 0,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetProcurementFunnelStatsQuery(companyId, year), ct);
        return Ok(ApiResponse<FunnelStatsDto>.Ok(result));
    }
}
