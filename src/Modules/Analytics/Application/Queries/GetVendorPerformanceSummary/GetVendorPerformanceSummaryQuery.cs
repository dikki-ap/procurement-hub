using MediatR;

namespace ProcureHub.Modules.Analytics.Application.Queries.GetVendorPerformanceSummary;

public record GetVendorPerformanceSummaryQuery(Guid CompanyId, int TopN = 10)
    : IRequest<VendorPerformanceSummaryDto>;
