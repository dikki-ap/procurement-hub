using FluentValidation;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateMaterialCategory;

public class UpdateMaterialCategoryCommandValidator : AbstractValidator<UpdateMaterialCategoryCommand>
{
    public UpdateMaterialCategoryCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Code).NotEmpty().MaximumLength(20);
    }
}
