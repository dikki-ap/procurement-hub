using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetRFQBids;

public record GetRFQBidsQuery(Guid RFQId) : IQuery<List<QuotationListDto>>;
