using FluentValidation;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.UpdateApprovalPolicy;

public class UpdateApprovalPolicyCommandValidator : AbstractValidator<UpdateApprovalPolicyCommand>
{
    public UpdateApprovalPolicyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ReferenceType).NotEmpty().Must(t => t is "PR" or "PO" or "RFQ")
            .WithMessage("ReferenceType must be PR, PO, or RFQ.");
        RuleFor(x => x.MinValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxValue).GreaterThan(x => x.MinValue)
            .When(x => x.MaxValue.HasValue)
            .WithMessage("MaxValue must be greater than MinValue.");
        RuleFor(x => x.RequiredLevels).InclusiveBetween(1, 5);
    }
}
