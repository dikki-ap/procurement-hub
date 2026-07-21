using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorDocumentDownloadUrl;

public class GetVendorDocumentDownloadUrlQueryHandler : IQueryHandler<GetVendorDocumentDownloadUrlQuery, VendorDocumentUrl>
{
    private readonly IVendorDocumentRepository _docRepo;
    private readonly IStorageService           _storage;

    public GetVendorDocumentDownloadUrlQueryHandler(IVendorDocumentRepository docRepo, IStorageService storage)
    {
        _docRepo = docRepo;
        _storage = storage;
    }

    public async Task<VendorDocumentUrl> Handle(GetVendorDocumentDownloadUrlQuery query, CancellationToken ct)
    {
        var doc = await _docRepo.GetByIdAsync(query.DocumentId, ct)
            ?? throw new NotFoundException("VendorDocument", query.DocumentId);

        if (doc.VendorId != query.VendorId)
            throw new NotFoundException("VendorDocument", query.DocumentId);

        var fileName    = doc.FileName ?? "document";
        var disposition = query.Inline
            ? "inline"
            : $"attachment; filename=\"{Uri.EscapeDataString(fileName)}\"";

        var url = await _storage.GetPresignedUrlAsync(
            "vendor-documents", doc.FileUrl, TimeSpan.FromMinutes(10), disposition, ct);

        return new VendorDocumentUrl(url, fileName);
    }
}
