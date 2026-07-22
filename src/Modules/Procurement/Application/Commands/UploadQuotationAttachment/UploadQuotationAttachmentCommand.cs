using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Commands.UploadQuotationAttachment;

public record UploadQuotationAttachmentCommand(
    Guid   QuotationId,
    Stream FileStream,
    string FileName,
    string ContentType) : ICommand<string>;
