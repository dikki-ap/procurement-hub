using ProcureHub.Modules.Procurement.Application.DTOs;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;

namespace ProcureHub.Modules.Procurement.Application.Queries.GetInvoiceList;

public class GetInvoiceListQueryHandler : IQueryHandler<GetInvoiceListQuery, List<InvoiceListDto>>
{
    private readonly IInvoiceRepository       _invoiceRepo;
    private readonly IPurchaseOrderRepository _poRepo;

    public GetInvoiceListQueryHandler(IInvoiceRepository invoiceRepo, IPurchaseOrderRepository poRepo)
    {
        _invoiceRepo = invoiceRepo;
        _poRepo      = poRepo;
    }

    public async Task<List<InvoiceListDto>> Handle(GetInvoiceListQuery query, CancellationToken ct)
    {
        var invoices = await _invoiceRepo.GetAllAsync(ct);
        return invoices.Select(i => new InvoiceListDto(
            i.Id,
            i.InvoiceNumber,
            i.POId,
            string.Empty,
            i.VendorId,
            i.VendorId.ToString(),
            i.Status,
            i.TotalAmount,
            i.DueAt,
            i.SubmittedAt,
            i.CreatedAt,
            i.CreatedBy?.FullName,
            i.UpdatedBy?.FullName,
            i.UpdatedAt)).ToList();
    }
}
