using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProcureHub.Modules.Analytics.Application.Queries.GetCycleTime;
using ProcureHub.Modules.Analytics.Application.Queries.GetDashboardWidgets;
using ProcureHub.Modules.Analytics.Application.Queries.GetProcurementFunnelStats;
using ProcureHub.Modules.Analytics.Application.Queries.GetSpendByCategory;
using ProcureHub.Modules.Analytics.Application.Queries.GetSpendSummary;
using ProcureHub.Modules.Analytics.Application.Queries.GetVendorConcentration;
using ProcureHub.Modules.Analytics.Application.Queries.GetVendorPerformanceSummary;
using ProcureHub.API.Services;
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
    private readonly IExcelExportService _excelExport;

    public DashboardController(
        IMediator mediator,
        ICurrentUserService currentUser,
        IExcelExportService excelExport)
    {
        _mediator    = mediator;
        _currentUser = currentUser;
        _excelExport = excelExport;
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

    /// <summary>Top 10 spend categories for a given year.</summary>
    [HttpGet("spend-by-category")]
    public async Task<IActionResult> GetSpendByCategory(
        [FromQuery] Guid companyId,
        [FromQuery] int  year = 0,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetSpendByCategoryQuery(companyId, year), ct);
        return Ok(ApiResponse<List<SpendByCategoryDto>>.Ok(result));
    }

    /// <summary>Average procurement cycle time per stage over the last N months.</summary>
    [HttpGet("cycle-time")]
    public async Task<IActionResult> GetCycleTime(
        [FromQuery] Guid companyId,
        [FromQuery] int  months = 3,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetCycleTimeQuery(companyId, months), ct);
        return Ok(ApiResponse<List<CycleTimeStageDto>>.Ok(result));
    }

    /// <summary>Top 5 vendors by spend share — flags single-vendor concentration risk.</summary>
    [HttpGet("vendor-concentration")]
    public async Task<IActionResult> GetVendorConcentration(
        [FromQuery] Guid companyId,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetVendorConcentrationQuery(companyId), ct);
        return Ok(ApiResponse<VendorConcentrationDto>.Ok(result));
    }

    /// <summary>Export spend report to Excel (monthly spend + category breakdown + invoice details).</summary>
    [HttpGet("spend-report/export")]
    public async Task<IActionResult> ExportSpendReport(
        [FromQuery] Guid      companyId,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to   = null,
        CancellationToken ct = default)
    {
        var today   = DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = from ?? new DateOnly(today.Year, 1, 1);
        var toDate   = to   ?? today;

        var bytes = await _excelExport.ExportSpendReportAsync(companyId, fromDate, toDate, ct);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"SpendReport_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx");
    }
}
