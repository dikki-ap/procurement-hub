using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetRFQAttachmentUrl;

public class GetRFQAttachmentUrlQueryHandler : IQueryHandler<GetRFQAttachmentUrlQuery, string>
{
    private const string Bucket = "rfq-attachments";

    private readonly IRFQRepository  _repo;
    private readonly IStorageService _storage;

    public GetRFQAttachmentUrlQueryHandler(IRFQRepository repo, IStorageService storage)
    {
        _repo    = repo;
        _storage = storage;
    }

    public async Task<string> Handle(GetRFQAttachmentUrlQuery query, CancellationToken ct)
    {
        var rfq = await _repo.GetByIdAsync(query.RFQId, ct)
            ?? throw new NotFoundException("RFQ", query.RFQId);

        if (string.IsNullOrEmpty(rfq.FileKey))
            throw new BusinessRuleException("RFQDownload", "No attachment has been uploaded for this RFQ.");

        var disposition = string.IsNullOrEmpty(rfq.FileName)
            ? "attachment"
            : $"attachment; filename=\"{rfq.FileName}\"";

        return await _storage.GetPresignedUrlAsync(Bucket, rfq.FileKey, TimeSpan.FromMinutes(30), disposition, ct);
    }
}
