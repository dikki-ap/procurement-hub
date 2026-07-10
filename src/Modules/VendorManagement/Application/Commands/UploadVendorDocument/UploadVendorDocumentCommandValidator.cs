using FluentValidation;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.UploadVendorDocument;

public class UploadVendorDocumentCommandValidator : AbstractValidator<UploadVendorDocumentCommand>
{
    private static readonly HashSet<string> AllowedTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // xlsx
        "application/vnd.ms-excel",                                           // xls
    ];

    private static readonly HashSet<string> AllowedExtensions =
        [".pdf", ".jpg", ".jpeg", ".png", ".xlsx", ".xls"];

    public UploadVendorDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255)
            .Must(name => AllowedExtensions.Contains(
                Path.GetExtension(name).ToLowerInvariant()))
            .WithMessage("Allowed file types: PDF, JPG, PNG, XLSX.");
        RuleFor(x => x.ContentType)
            .Must(t => AllowedTypes.Contains(t))
            .WithMessage("Allowed content types: PDF, JPEG, PNG, XLSX.");
        RuleFor(x => x.FileStream)
            .Must((cmd, s) => s.Length <= (long)cmd.MaxFileSizeMb * 1024 * 1024)
            .WithMessage(cmd => $"File size must not exceed {cmd.MaxFileSizeMb} MB.");
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes != null);
    }
}
