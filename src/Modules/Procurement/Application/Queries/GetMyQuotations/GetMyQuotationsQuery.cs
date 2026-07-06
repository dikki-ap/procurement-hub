using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetMyQuotations;

public record GetMyQuotationsQuery(Guid VendorId) : IQuery<List<QuotationListDto>>;
