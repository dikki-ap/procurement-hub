using FluentValidation;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateMaterial;

public class UpdateMaterialCommandValidator : AbstractValidator<UpdateMaterialCommand>
{
    public UpdateMaterialCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.CategoryId).NotEmpty();
        RuleFor(c => c.Code).NotEmpty().MaximumLength(30);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(255);
        RuleFor(c => c.UomId).NotEmpty();
        RuleFor(c => c.EstimatedPrice).GreaterThanOrEqualTo(0).When(c => c.EstimatedPrice.HasValue);
        RuleFor(c => c.CurrencyId).NotEmpty().When(c => c.EstimatedPrice.HasValue)
            .WithMessage("CurrencyId is required when EstimatedPrice is provided");
    }
}
