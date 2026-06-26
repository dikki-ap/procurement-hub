using FluentValidation;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdatePaymentTerm;

public class UpdatePaymentTermCommandValidator : AbstractValidator<UpdatePaymentTermCommand>
{
    public UpdatePaymentTermCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Code).NotEmpty().MaximumLength(20);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Days).GreaterThanOrEqualTo(0);
    }
}
