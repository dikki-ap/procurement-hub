using FluentValidation;

namespace ProcureHub.Modules.Procurement.Application.Commands.SubmitQuotation;

public class SubmitQuotationCommandValidator : AbstractValidator<SubmitQuotationCommand>
{
    public SubmitQuotationCommandValidator()
    {
        RuleFor(c => c.RFQId).NotEmpty();
        RuleFor(c => c.VendorId).NotEmpty();
        RuleFor(c => c.Items).NotEmpty().WithMessage("At least one item must be quoted.");
        RuleForEach(c => c.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.RFQItemId).NotEmpty();
            item.RuleFor(i => i.UnitPrice).GreaterThan(0).WithMessage("Unit price must be greater than zero.");
        });
    }
}
