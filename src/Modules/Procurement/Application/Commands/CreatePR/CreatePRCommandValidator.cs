using FluentValidation;

namespace ProcureHub.Modules.Procurement.Application.Commands.CreatePR;

public class CreatePRCommandValidator : AbstractValidator<CreatePRCommand>
{
    public CreatePRCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Department).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RequiredDate).GreaterThan(DateTime.UtcNow)
            .WithMessage("Required date must be in the future.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("PR must have at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ItemDescription).NotEmpty().MaximumLength(500);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.EstimatedUnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
