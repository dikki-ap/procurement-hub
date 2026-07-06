using ProcureHub.Modules.Procurement.Domain.Enums;

namespace ProcureHub.Modules.Procurement.Application.DTOs;

public record BidEvaluationDto(
    Guid                     Id,
    Guid                     RFQId,
    decimal                  PriceWeight,
    decimal                  QualityWeight,
    decimal                  DeliveryWeight,
    EvaluationStatus         Status,
    Guid?                    AwardedVendorId,
    Guid?                    AwardedQuotationId,
    List<EvaluationScoreDto> Scores);

public record EvaluationScoreDto(
    Guid    QuotationId,
    Guid    VendorId,
    string  VendorName,
    decimal PriceScore,
    decimal QualityScore,
    decimal DeliveryScore,
    decimal WeightedTotal);
