using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorDocumentDownloadUrl;

public record VendorDocumentStream(Stream Content, string ContentType, string? FileName);

public record GetVendorDocumentDownloadUrlQuery(Guid VendorId, Guid DocumentId) : IQuery<VendorDocumentStream>;
