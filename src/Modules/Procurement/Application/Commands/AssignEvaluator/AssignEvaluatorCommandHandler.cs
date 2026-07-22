using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.AssignEvaluator;

public class AssignEvaluatorCommandHandler : ICommandHandler<AssignEvaluatorCommand, Guid>
{
    private readonly IBidEvaluationRepository _evalRepo;
    private readonly INotificationService     _notifications;

    public AssignEvaluatorCommandHandler(
        IBidEvaluationRepository evalRepo,
        INotificationService     notifications)
    {
        _evalRepo      = evalRepo;
        _notifications = notifications;
    }

    public async Task<Guid> Handle(AssignEvaluatorCommand cmd, CancellationToken ct)
    {
        var evaluation = await _evalRepo.GetByRFQIdWithEvaluatorsAsync(cmd.RFQId, ct)
            ?? throw new NotFoundException("BidEvaluation for RFQ", cmd.RFQId);

        if (evaluation.Status == EvaluationStatus.Awarded)
            throw new BusinessRuleException("AssignEvaluator",
                "Cannot assign evaluators to an already-awarded evaluation.");

        if (evaluation.Evaluators.Any(e => e.AssignedUserId == cmd.UserId))
            throw new BusinessRuleException("AssignEvaluator",
                "This user is already assigned as an evaluator for this evaluation.");

        var assignment = new EvaluatorAssignment
        {
            BidEvaluationId  = evaluation.Id,
            AssignedUserId   = cmd.UserId,
            AssignedUserName = cmd.UserName,
            HasSubmitted     = false,
        };
        evaluation.Evaluators.Add(assignment);

        if (evaluation.Status == EvaluationStatus.Pending)
            evaluation.Status = EvaluationStatus.InProgress;

        _evalRepo.Update(evaluation);
        await _evalRepo.SaveChangesAsync(ct);

        await _notifications.SendAsync(
            cmd.UserId,
            "Bid Evaluation Assignment",
            "You have been assigned to evaluate bids. Please submit your scores.",
            ct: ct);

        return assignment.Id;
    }
}
