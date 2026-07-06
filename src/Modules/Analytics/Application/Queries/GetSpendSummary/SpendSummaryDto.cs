namespace ProcureHub.Modules.Analytics.Application.Queries.GetSpendSummary;

public record SpendSummaryDto(List<MonthlySpendDto> Monthly, decimal TotalThisYear, decimal TotalLastYear);

public record MonthlySpendDto(string Month, decimal Amount);
