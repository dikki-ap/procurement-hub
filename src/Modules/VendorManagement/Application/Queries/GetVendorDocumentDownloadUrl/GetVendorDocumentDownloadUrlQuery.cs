using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorDocumentDownloadUrl;

public record GetVendorDocumentDownloadUrlQuery(Guid VendorId, Guid DocumentId) : IQuery<string>;
