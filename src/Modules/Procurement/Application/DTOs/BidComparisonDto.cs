namespace ProcureHub.Modules.Procurement.Application.DTOs;

public record BidComparisonDto(
    Guid                       RFQId,
    string                     RFQNumber,
    string                     RFQTitle,
    List<BidComparisonItemDto> Items,
    List<BidComparisonRowDto>  Vendors);

public record BidComparisonItemDto(
    Guid    RFQItemId,
    string  ItemDescription,
    decimal Quantity,
    string? UnitLabel);

public record BidComparisonRowDto(
    Guid                         VendorId,
    string                       VendorName,
    Guid                         QuotationId,
    string                       Status,
    decimal                      TotalPrice,
    List<BidComparisonPriceDto>  ItemPrices);

public record BidComparisonPriceDto(
    Guid    RFQItemId,
    decimal UnitPrice,
    decimal LineTotal,
    string? Notes);
