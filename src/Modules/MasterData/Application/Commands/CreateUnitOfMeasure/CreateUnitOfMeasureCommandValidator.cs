using FluentValidation;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateUnitOfMeasure;

public class CreateUnitOfMeasureCommandValidator : AbstractValidator<CreateUnitOfMeasureCommand>
{
    public CreateUnitOfMeasureCommandValidator()
    {
        RuleFor(c => c.CompanyId).NotEmpty();
        RuleFor(c => c.Code).NotEmpty().MaximumLength(10);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(50);
    }
}
