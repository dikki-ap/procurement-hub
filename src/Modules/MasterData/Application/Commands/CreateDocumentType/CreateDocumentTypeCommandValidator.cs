using FluentValidation;

namespace ProcureHub.Modules.MasterData.Application.Commands.CreateDocumentType;

public class CreateDocumentTypeCommandValidator : AbstractValidator<CreateDocumentTypeCommand>
{
    public CreateDocumentTypeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MaxFileSizeMb).InclusiveBetween(1, 100);
    }
}
