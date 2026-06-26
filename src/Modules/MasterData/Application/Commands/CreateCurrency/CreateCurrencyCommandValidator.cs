using FluentValidation;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateCurrency;

public class CreateCurrencyCommandValidator : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyCommandValidator()
    {
        RuleFor(c => c.Code).NotEmpty().MaximumLength(5);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Symbol).MaximumLength(5).When(c => c.Symbol is not null);
        RuleFor(c => c.ExchangeRate).GreaterThan(0);
    }
}
