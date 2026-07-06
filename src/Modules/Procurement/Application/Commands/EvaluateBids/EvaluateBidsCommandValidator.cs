using FluentValidation;

namespace ProcureHub.Modules.Procurement.Application.Commands.EvaluateBids;

public class EvaluateBidsCommandValidator : AbstractValidator<EvaluateBidsCommand>
{
    public EvaluateBidsCommandValidator()
    {
        RuleFor(c => c.RFQId).NotEmpty();
        RuleFor(c => c.PriceWeight).InclusiveBetween(0m, 100m);
        RuleFor(c => c.QualityWeight).InclusiveBetween(0m, 100m);
        RuleFor(c => c.DeliveryWeight).InclusiveBetween(0m, 100m);
        RuleFor(c => c)
            .Must(c => Math.Abs(c.PriceWeight + c.QualityWeight + c.DeliveryWeight - 100m) <= 0.01m)
            .WithMessage("Weights must sum to 100.");
        RuleFor(c => c.Scores).NotEmpty().WithMessage("At least one vendor score is required.");
        RuleForEach(c => c.Scores).ChildRules(s =>
        {
            s.RuleFor(i => i.QualityScore).InclusiveBetween(0m, 100m);
            s.RuleFor(i => i.DeliveryScore).InclusiveBetween(0m, 100m);
        });
    }
}
