using FluentValidation;

namespace ProcureHub.Modules.MasterData.Application.Commands.UpdateDocumentType;

public class UpdateDocumentTypeCommandValidator : AbstractValidator<UpdateDocumentTypeCommand>
{
    public UpdateDocumentTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MaxFileSizeMb).InclusiveBetween(1, 100);
    }
}
