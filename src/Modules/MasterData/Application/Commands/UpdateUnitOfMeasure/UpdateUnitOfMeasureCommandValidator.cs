using FluentValidation;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateUnitOfMeasure;

public class UpdateUnitOfMeasureCommandValidator : AbstractValidator<UpdateUnitOfMeasureCommand>
{
    public UpdateUnitOfMeasureCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Code).NotEmpty().MaximumLength(10);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(50);
    }
}
