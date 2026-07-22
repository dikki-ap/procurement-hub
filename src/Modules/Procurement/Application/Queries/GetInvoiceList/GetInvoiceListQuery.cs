using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetInvoiceList;

public record GetInvoiceListQuery(Guid CompanyId) : IQuery<List<InvoiceListDto>>;
