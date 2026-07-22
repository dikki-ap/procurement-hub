using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetRFQAttachmentUrl;

public record GetRFQAttachmentUrlQuery(Guid RFQId) : IQuery<string>;
