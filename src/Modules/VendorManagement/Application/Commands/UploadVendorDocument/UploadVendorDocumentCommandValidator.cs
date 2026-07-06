using FluentValidation;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.UploadVendorDocument;

public class UploadVendorDocumentCommandValidator : AbstractValidator<UploadVendorDocumentCommand>
{
    private static readonly HashSet<string> AllowedTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // xlsx
        "application/vnd.ms-excel",                                           // xls
    ];

    private static readonly HashSet<string> AllowedExtensions =
        [".pdf", ".jpg", ".jpeg", ".png", ".webp", ".xlsx", ".xls"];

    public UploadVendorDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentType).IsInEnum();
        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255)
            .Must(name => AllowedExtensions.Contains(
                Path.GetExtension(name).ToLowerInvariant()))
            .WithMessage("Allowed file types: PDF, JPG, PNG, WebP, XLSX.");
        RuleFor(x => x.ContentType)
            .Must(t => AllowedTypes.Contains(t))
            .WithMessage("Allowed content types: PDF, JPEG, PNG, WebP, XLSX.");
        RuleFor(x => x.FileStream)
            .Must(s => s.Length <= 10 * 1024 * 1024)
            .WithMessage("File size must not exceed 10 MB.");
        RuleFor(x => x.Notes).MaximumLength(500).When(x => x.Notes != null);
    }
}
