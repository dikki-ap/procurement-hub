using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.VendorManagement.Application.Queries.GetVendorDocumentDownloadUrl;

public class GetVendorDocumentDownloadUrlQueryHandler : IQueryHandler<GetVendorDocumentDownloadUrlQuery, VendorDocumentStream>
{
    private readonly IVendorDocumentRepository _docRepo;
    private readonly IStorageService           _storage;

    public GetVendorDocumentDownloadUrlQueryHandler(IVendorDocumentRepository docRepo, IStorageService storage)
    {
        _docRepo = docRepo;
        _storage = storage;
    }

    public async Task<VendorDocumentStream> Handle(GetVendorDocumentDownloadUrlQuery query, CancellationToken ct)
    {
        var doc = await _docRepo.GetByIdAsync(query.DocumentId, ct)
            ?? throw new NotFoundException("VendorDocument", query.DocumentId);

        if (doc.VendorId != query.VendorId)
            throw new NotFoundException("VendorDocument", query.DocumentId);

        var (stream, contentType) = await _storage.DownloadAsync("vendor-documents", doc.FileUrl, ct);

        return new VendorDocumentStream(stream, contentType, doc.FileName);
    }
}
