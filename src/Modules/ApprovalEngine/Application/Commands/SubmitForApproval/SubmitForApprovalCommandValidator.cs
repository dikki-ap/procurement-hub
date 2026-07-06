using FluentValidation;

namespace ProcureHub.Modules.ApprovalEngine.Application.Commands.SubmitForApproval;

public class SubmitForApprovalCommandValidator : AbstractValidator<SubmitForApprovalCommand>
{
    public SubmitForApprovalCommandValidator()
    {
        RuleFor(x => x.ReferenceType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ReferenceId).NotEmpty();
        RuleFor(x => x.ReferenceNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ReferenceTitle).NotEmpty().MaximumLength(300);
        RuleFor(x => x.TotalValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RequestedById).NotEmpty();
        RuleFor(x => x.Approvers).NotEmpty();
        RuleForEach(x => x.Approvers).ChildRules(a =>
        {
            a.RuleFor(x => x.Level).GreaterThan(0);
            a.RuleFor(x => x.UserId).NotEmpty();
            a.RuleFor(x => x.UserName).NotEmpty();
        });
    }
}
