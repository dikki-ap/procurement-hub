using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.UploadRFQAttachment;

public record UploadRFQAttachmentCommand(
    Guid   RFQId,
    Stream FileStream,
    string FileName,
    string ContentType) : ICommand<string>;
