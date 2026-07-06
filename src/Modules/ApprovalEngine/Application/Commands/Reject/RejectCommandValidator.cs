using FluentValidation;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.Reject;

public class RejectCommandValidator : AbstractValidator<RejectCommand>
{
    public RejectCommandValidator()
    {
        RuleFor(x => x.WorkflowId).NotEmpty();
        RuleFor(x => x.ApproverId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(20)
            .WithMessage("Reason must be at least 20 characters.");
    }
}
