using FluentValidation;

namespace ProcureHub.Modules.Procurement.Application.Commands.CreateRFQ;

public class CreateRFQCommandValidator : AbstractValidator<CreateRFQCommand>
{
    public CreateRFQCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.BidDeadline).GreaterThan(DateTime.UtcNow)
            .WithMessage("Bid deadline must be in the future.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("RFQ must have at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ItemDescription).NotEmpty().MaximumLength(500);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}
