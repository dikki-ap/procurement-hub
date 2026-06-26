using FluentValidation;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateLocation;

public class UpdateLocationCommandValidator : AbstractValidator<UpdateLocationCommand>
{
    private static readonly string[] ValidTypes = ["warehouse", "plant", "office"];

    public UpdateLocationCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Type).NotEmpty().Must(t => ValidTypes.Contains(t.ToLower()))
            .WithMessage("Type must be 'warehouse', 'plant', or 'office'");
        RuleFor(c => c.Country).NotEmpty().MaximumLength(100);
    }
}
