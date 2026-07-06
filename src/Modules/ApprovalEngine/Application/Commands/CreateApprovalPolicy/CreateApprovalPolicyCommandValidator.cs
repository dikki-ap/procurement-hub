using FluentValidation;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.CreateApprovalPolicy;

public class CreateApprovalPolicyCommandValidator : AbstractValidator<CreateApprovalPolicyCommand>
{
    public CreateApprovalPolicyCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ReferenceType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MinValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RequiredLevels).InclusiveBetween(1, 5);
    }
}
