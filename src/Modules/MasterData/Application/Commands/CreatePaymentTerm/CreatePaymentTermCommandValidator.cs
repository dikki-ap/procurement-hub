using FluentValidation;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreatePaymentTerm;

public class CreatePaymentTermCommandValidator : AbstractValidator<CreatePaymentTermCommand>
{
    public CreatePaymentTermCommandValidator()
    {
        RuleFor(c => c.CompanyId).NotEmpty();
        RuleFor(c => c.Code).NotEmpty().MaximumLength(20);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Days).GreaterThanOrEqualTo(0);
    }
}
