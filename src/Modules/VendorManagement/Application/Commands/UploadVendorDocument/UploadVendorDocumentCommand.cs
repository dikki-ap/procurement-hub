using ProcureHub.Modules.VendorManagement.Domain.Enums;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.UploadVendorDocument;

public record UploadVendorDocumentCommand(
    Guid      VendorId,
    string    DocumentType,
    string?   DocumentNumber,
    Stream    FileStream,
    string    FileName,
    string    ContentType,
    int       MaxFileSizeMb,
    DateOnly? ExpiredAt,
    DateOnly? IssuedAt,
    string?   Notes
) : ICommand<Guid>;
