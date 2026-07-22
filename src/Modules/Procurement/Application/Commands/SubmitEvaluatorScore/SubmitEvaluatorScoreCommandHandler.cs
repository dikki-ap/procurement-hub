using MediatR;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.SubmitEvaluatorScore;

public class SubmitEvaluatorScoreCommandHandler : ICommandHandler<SubmitEvaluatorScoreCommand>
{
    private readonly IBidEvaluationRepository _evalRepo;

    public SubmitEvaluatorScoreCommandHandler(IBidEvaluationRepository evalRepo)
        => _evalRepo = evalRepo;

    public async Task<Unit> Handle(SubmitEvaluatorScoreCommand cmd, CancellationToken ct)
    {
        var evaluation = await _evalRepo.GetByRFQIdFullAsync(cmd.RFQId, ct)
            ?? throw new NotFoundException("BidEvaluation for RFQ", cmd.RFQId);

        if (evaluation.Status == EvaluationStatus.Awarded)
            throw new BusinessRuleException("SubmitEvaluatorScore",
                "Evaluation has already been awarded.");

        var assignment = evaluation.Evaluators
            .FirstOrDefault(e => e.AssignedUserId == cmd.EvaluatorUserId)
            ?? throw new ForbiddenException("You are not assigned as an evaluator for this bid.");

        if (assignment.HasSubmitted)
            throw new BusinessRuleException("SubmitEvaluatorScore",
                "You have already submitted your scores for this evaluation.");

        foreach (var input in cmd.Scores)
        {
            if (input.QualityScore  is < 0 or > 100)
                throw new BusinessRuleException("SubmitEvaluatorScore", "Quality score must be between 0 and 100.");
            if (input.DeliveryScore is < 0 or > 100)
                throw new BusinessRuleException("SubmitEvaluatorScore", "Delivery score must be between 0 and 100.");

            // Replace existing score for this quotation if any (re-submission before first HasSubmitted)
            var existing = assignment.Scores.FirstOrDefault(s => s.QuotationId == input.QuotationId);
            if (existing is not null)
            {
                existing.QualityScore  = input.QualityScore;
                existing.DeliveryScore = input.DeliveryScore;
            }
            else
            {
                assignment.Scores.Add(new EvaluatorScore
                {
                    EvaluatorAssignmentId = assignment.Id,
                    QuotationId           = input.QuotationId,
                    QualityScore          = input.QualityScore,
                    DeliveryScore         = input.DeliveryScore,
                });
            }
        }

        assignment.HasSubmitted = true;
        _evalRepo.Update(evaluation);
        await _evalRepo.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
