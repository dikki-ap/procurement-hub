using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetInvoiceList;

public record GetInvoiceListQuery : IQuery<List<InvoiceListDto>>;
