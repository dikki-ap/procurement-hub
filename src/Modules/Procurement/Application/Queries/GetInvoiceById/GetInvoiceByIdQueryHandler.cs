using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryHandler : IQueryHandler<GetInvoiceByIdQuery, InvoiceDto>
{
    private readonly IInvoiceRepository _repo;

    public GetInvoiceByIdQueryHandler(IInvoiceRepository repo) => _repo = repo;

    public async Task<InvoiceDto> Handle(GetInvoiceByIdQuery query, CancellationToken ct)
    {
        var i = await _repo.GetByIdAsync(query.Id, ct)
                ?? throw new NotFoundException("Invoice", query.Id);

        return new InvoiceDto(
            i.Id,
            i.InvoiceNumber,
            i.POId,
            string.Empty,
            i.VendorId,
            i.VendorId.ToString(),
            i.Status,
            i.Amount,
            i.TaxAmount,
            i.TotalAmount,
            null,
            i.FileUrl,
            i.DueAt,
            i.PaidAt,
            i.PaymentReference,
            i.Notes,
            i.RejectionReason,
            i.SubmittedAt,
            i.ReviewedAt);
    }
}
