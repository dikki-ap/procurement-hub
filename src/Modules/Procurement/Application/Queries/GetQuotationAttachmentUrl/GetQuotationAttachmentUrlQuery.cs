using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetQuotationAttachmentUrl;

public record GetQuotationAttachmentUrlQuery(Guid QuotationId) : IQuery<string>;
