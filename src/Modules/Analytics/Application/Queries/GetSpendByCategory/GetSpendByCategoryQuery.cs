using MediatR;

namespace ProcureHub.Modules.Analytics.Application.Queries.GetSpendByCategory;

public record GetSpendByCategoryQuery(Guid CompanyId, int Year) : IRequest<List<SpendByCategoryDto>>;

public record SpendByCategoryDto(
    string  CategoryName,
    decimal TotalSpend,
    decimal PctOfTotal);
