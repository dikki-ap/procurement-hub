using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.EvaluateBids;

public record VendorScoreInput(
    Guid    QuotationId,
    Guid    VendorId,
    decimal QualityScore,
    decimal DeliveryScore);

public record EvaluateBidsCommand(
    Guid                   RFQId,
    decimal                PriceWeight,
    decimal                QualityWeight,
    decimal                DeliveryWeight,
    List<VendorScoreInput> Scores) : ICommand<Guid>;
