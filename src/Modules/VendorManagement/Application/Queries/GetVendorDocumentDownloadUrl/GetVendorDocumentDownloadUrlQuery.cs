using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorDocumentDownloadUrl;

public record VendorDocumentUrl(string Url, string FileName);

public record GetVendorDocumentDownloadUrlQuery(Guid VendorId, Guid DocumentId, bool Inline = false)
    : IQuery<VendorDocumentUrl>;
