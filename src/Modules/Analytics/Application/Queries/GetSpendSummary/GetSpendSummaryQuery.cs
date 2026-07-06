using MediatR;

namespace ProcureHub.Modules.Analytics.Application.Queries.GetSpendSummary;

public record GetSpendSummaryQuery(Guid CompanyId, int Months = 12) : IRequest<SpendSummaryDto>;
