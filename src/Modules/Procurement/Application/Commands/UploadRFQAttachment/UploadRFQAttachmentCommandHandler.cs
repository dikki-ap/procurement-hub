using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.UploadRFQAttachment;

public class UploadRFQAttachmentCommandHandler : ICommandHandler<UploadRFQAttachmentCommand, string>
{
    private const string Bucket = "rfq-attachments";

    private readonly IRFQRepository  _repo;
    private readonly IStorageService _storage;

    public UploadRFQAttachmentCommandHandler(IRFQRepository repo, IStorageService storage)
    {
        _repo    = repo;
        _storage = storage;
    }

    public async Task<string> Handle(UploadRFQAttachmentCommand cmd, CancellationToken ct)
    {
        var rfq = await _repo.GetByIdAsync(cmd.RFQId, ct)
            ?? throw new NotFoundException("RFQ", cmd.RFQId);

        var ext       = Path.GetExtension(cmd.FileName);
        var objectKey = $"rfqs/{rfq.CompanyId}/{rfq.Id}/{Guid.NewGuid()}{ext}";

        await _storage.EnsureBucketExistsAsync(Bucket, ct);

        if (!string.IsNullOrEmpty(rfq.FileKey))
        {
            try { await _storage.DeleteAsync(Bucket, rfq.FileKey, ct); }
            catch { /* tolerate stale key */ }
        }

        var key = await _storage.UploadAsync(Bucket, objectKey, cmd.FileStream, cmd.ContentType, ct);

        rfq.FileKey  = key;
        rfq.FileName = cmd.FileName;
        _repo.Update(rfq);
        await _repo.SaveChangesAsync(ct);
        return key;
    }
}
