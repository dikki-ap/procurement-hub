using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.Modules.VendorManagement.Domain.Repositories;
using ProcureHub.Modules.VendorManagement.Infrastructure.Storage;
using ProcureHub.SharedKernel.Caching;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.VendorManagement.Application.Commands.UploadVendorDocument;

public class UploadVendorDocumentCommandHandler : ICommandHandler<UploadVendorDocumentCommand, Guid>
{
    private readonly IVendorRepository         _vendorRepo;
    private readonly IVendorDocumentRepository _docRepo;
    private readonly IStorageService           _storage;
    private readonly ICacheService             _cache;

    public UploadVendorDocumentCommandHandler(
        IVendorRepository         vendorRepo,
        IVendorDocumentRepository docRepo,
        IStorageService           storage,
        ICacheService             cache)
    {
        _vendorRepo = vendorRepo;
        _docRepo    = docRepo;
        _storage    = storage;
        _cache      = cache;
    }

    public async Task<Guid> Handle(UploadVendorDocumentCommand command, CancellationToken ct)
    {
        var vendor = await _vendorRepo.GetByIdAsync(command.VendorId, ct)
            ?? throw new NotFoundException("Vendor", command.VendorId);

        var safeFileName = SanitizeFileName(command.FileName);
        var objectKey    = $"vendors/{vendor.Id}/documents/{Guid.NewGuid():N}_{safeFileName}";
        var fileUrl   = await _storage.UploadAsync("vendor-documents", objectKey, command.FileStream, command.ContentType, ct);

        var doc = new VendorDocument
        {
            VendorId       = vendor.Id,
            DocumentType   = command.DocumentType,
            DocumentNumber = command.DocumentNumber,
            FileUrl        = fileUrl,
            FileName       = command.FileName,
            FileSize       = command.FileStream.Length,
            ExpiredAt      = command.ExpiredAt,
            IssuedAt       = command.IssuedAt,
            Notes          = command.Notes,
        };

        _docRepo.Add(doc);
        await _docRepo.SaveChangesAsync(ct);

        _cache.RemoveByPrefix(CacheKeys.Vendors.Prefix);

        return doc.Id;
    }

    private static string SanitizeFileName(string fileName)
    {
        // Strip directory traversal, keep only the bare filename
        var name = Path.GetFileName(fileName);

        // Replace any remaining invalid or risky characters with underscores
        var invalid = Path.GetInvalidFileNameChars();
        foreach (var ch in invalid)
            name = name.Replace(ch, '_');

        // Guard against empty result after sanitization
        return string.IsNullOrWhiteSpace(name) ? "upload" : name;
    }
}
