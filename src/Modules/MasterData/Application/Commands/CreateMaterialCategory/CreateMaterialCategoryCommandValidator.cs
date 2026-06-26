using FluentValidation;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateMaterialCategory;

public class CreateMaterialCategoryCommandValidator : AbstractValidator<CreateMaterialCategoryCommand>
{
    public CreateMaterialCategoryCommandValidator()
    {
        RuleFor(c => c.CompanyId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Code).NotEmpty().MaximumLength(20);
    }
}
