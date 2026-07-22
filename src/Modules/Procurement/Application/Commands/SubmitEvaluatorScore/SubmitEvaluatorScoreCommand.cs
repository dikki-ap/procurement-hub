using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.SubmitEvaluatorScore;

public record SubmitEvaluatorScoreCommand(
    Guid                      RFQId,
    Guid                      EvaluatorUserId,
    List<EvaluatorScoreInput> Scores) : ICommand;

public record EvaluatorScoreInput(
    Guid    QuotationId,
    decimal QualityScore,
    decimal DeliveryScore);
