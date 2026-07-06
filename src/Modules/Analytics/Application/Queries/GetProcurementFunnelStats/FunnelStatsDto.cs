namespace ProcureHub.Modules.Analytics.Application.Queries.GetProcurementFunnelStats;

public record FunnelStatsDto(List<FunnelStageDto> Stages);

public record FunnelStageDto(string Stage, int Count, decimal Value);
