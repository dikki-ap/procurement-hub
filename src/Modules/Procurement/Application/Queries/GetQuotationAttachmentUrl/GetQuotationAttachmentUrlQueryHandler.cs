using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.Abstractions;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetQuotationAttachmentUrl;

public class GetQuotationAttachmentUrlQueryHandler : IQueryHandler<GetQuotationAttachmentUrlQuery, string>
{
    private const string Bucket = "quotation-attachments";

    private readonly IVendorQuotationRepository _repo;
    private readonly IStorageService            _storage;

    public GetQuotationAttachmentUrlQueryHandler(IVendorQuotationRepository repo, IStorageService storage)
    {
        _repo    = repo;
        _storage = storage;
    }

    public async Task<string> Handle(GetQuotationAttachmentUrlQuery query, CancellationToken ct)
    {
        var quotation = await _repo.GetByIdAsync(query.QuotationId, ct)
            ?? throw new NotFoundException("VendorQuotation", query.QuotationId);

        if (string.IsNullOrEmpty(quotation.FileKey))
            throw new BusinessRuleException("QuotationDownload", "No attachment has been uploaded for this quotation.");

        var disposition = string.IsNullOrEmpty(quotation.FileName)
            ? "attachment"
            : $"attachment; filename=\"{quotation.FileName}\"";

        return await _storage.GetPresignedUrlAsync(Bucket, quotation.FileKey, TimeSpan.FromMinutes(30), disposition, ct);
    }
}
