namespace ProcureHub.Modules.Analytics.Application.Queries.GetVendorPerformanceSummary;

public record VendorPerformanceSummaryDto(List<VendorPerformanceDto> Vendors);

public record VendorPerformanceDto(
    Guid    VendorId,
    string  VendorName,
    string  Tier,
    decimal TotalScore,
    decimal DeliveryScore,
    decimal QualityScore,
    decimal TotalSpend,
    int     POCount
);
