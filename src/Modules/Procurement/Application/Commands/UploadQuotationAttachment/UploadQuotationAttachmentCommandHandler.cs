using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.UploadQuotationAttachment;

public class UploadQuotationAttachmentCommandHandler : ICommandHandler<UploadQuotationAttachmentCommand, string>
{
    private const string Bucket = "quotation-attachments";

    private readonly IVendorQuotationRepository _repo;
    private readonly IStorageService            _storage;

    public UploadQuotationAttachmentCommandHandler(IVendorQuotationRepository repo, IStorageService storage)
    {
        _repo    = repo;
        _storage = storage;
    }

    public async Task<string> Handle(UploadQuotationAttachmentCommand cmd, CancellationToken ct)
    {
        var quotation = await _repo.GetByIdAsync(cmd.QuotationId, ct)
            ?? throw new NotFoundException("VendorQuotation", cmd.QuotationId);

        var ext       = Path.GetExtension(cmd.FileName);
        var objectKey = $"quotations/{quotation.VendorId}/{quotation.Id}/{Guid.NewGuid()}{ext}";

        await _storage.EnsureBucketExistsAsync(Bucket, ct);

        if (!string.IsNullOrEmpty(quotation.FileKey))
        {
            try { await _storage.DeleteAsync(Bucket, quotation.FileKey, ct); }
            catch { /* tolerate stale key */ }
        }

        var key = await _storage.UploadAsync(Bucket, objectKey, cmd.FileStream, cmd.ContentType, ct);

        quotation.FileKey  = key;
        quotation.FileName = cmd.FileName;
        _repo.Update(quotation);
        await _repo.SaveChangesAsync(ct);
        return key;
    }
}
