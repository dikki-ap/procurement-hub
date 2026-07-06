using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetInvoiceById;

public record GetInvoiceByIdQuery(Guid Id) : IQuery<InvoiceDto>;
