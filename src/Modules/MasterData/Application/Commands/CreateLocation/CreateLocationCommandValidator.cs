using FluentValidation;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateLocation;

public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    private static readonly string[] ValidTypes = ["warehouse", "plant", "office"];

    public CreateLocationCommandValidator()
    {
        RuleFor(c => c.CompanyId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Type).NotEmpty().Must(t => ValidTypes.Contains(t.ToLower()))
            .WithMessage("Type must be 'warehouse', 'plant', or 'office'");
        RuleFor(c => c.Country).NotEmpty().MaximumLength(100);
    }
}
