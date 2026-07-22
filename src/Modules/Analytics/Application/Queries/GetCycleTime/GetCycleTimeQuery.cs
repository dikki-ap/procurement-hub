using MediatR;

namespace ProcureHub.Modules.Analytics.Application.Queries.GetCycleTime;

public record GetCycleTimeQuery(Guid CompanyId, int Months = 3) : IRequest<List<CycleTimeStageDto>>;

public record CycleTimeStageDto(string Stage, double AvgDays);
