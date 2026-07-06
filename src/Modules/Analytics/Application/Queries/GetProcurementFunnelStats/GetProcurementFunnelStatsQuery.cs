using MediatR;

namespace ProcureHub.Modules.Analytics.Application.Queries.GetProcurementFunnelStats;

public record GetProcurementFunnelStatsQuery(Guid CompanyId, int Year = 0)
    : IRequest<FunnelStatsDto>;
