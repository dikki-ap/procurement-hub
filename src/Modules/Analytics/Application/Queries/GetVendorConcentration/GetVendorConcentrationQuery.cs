using MediatR;

namespace ProcureHub.Modules.Analytics.Application.Queries.GetVendorConcentration;

public record GetVendorConcentrationQuery(Guid CompanyId) : IRequest<VendorConcentrationDto>;

public record VendorConcentrationDto(
    List<VendorSpendShareDto> TopVendors,
    bool HasConcentrationRisk);

public record VendorSpendShareDto(
    Guid    VendorId,
    string  VendorName,
    decimal TotalSpend,
    decimal PctOfTotal);
