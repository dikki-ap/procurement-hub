using MediatR;
using ProcureHub.Modules.DocumentManagement.Application.Services;
using ProcureHub.Modules.Procurement.Domain.Repositories;
using ProcureHub.SharedKernel.CQRS;
using ProcureHub.SharedKernel.Exceptions;

namespace ProcureHub.Modules.Procurement.Application.Commands.IssuePO;

public class IssuePOCommandHandler : ICommandHandler<IssuePOCommand>
{
    private readonly IPurchaseOrderRepository _repo;
    private readonly IPdfGeneratorService     _pdfService;

    public IssuePOCommandHandler(
        IPurchaseOrderRepository repo,
        IPdfGeneratorService pdfService)
    {
        _repo       = repo;
        _pdfService = pdfService;
    }

    public async Task<Unit> Handle(IssuePOCommand command, CancellationToken ct)
    {
        var po = await _repo.GetByIdWithItemsAsync(command.POId, ct)
                 ?? throw new NotFoundException("PurchaseOrder", command.POId);

        var pdfData = new PurchaseOrderPdfData(
            PoNumber:         po.PONumber,
            CompanyName:      "ProcureHub Company",
            VendorName:       po.VendorId.ToString(),
            VendorAddress:    string.Empty,
            Currency:         "IDR",
            IssuedAt:         DateTime.UtcNow,
            ExpectedDelivery: po.ExpectedDelivery,
            PaymentTerms:     null,
            DeliveryLocation: null,
            Notes:            po.Notes,
            TotalAmount:      po.TotalAmount,
            Items: po.Items.Select(i => new POItemPdfRow(
                i.Description,
                i.Quantity,
                "pcs",
                i.UnitPrice,
                i.TotalPrice)).ToList());

        var pdfBytes = await _pdfService.GeneratePurchaseOrderPdfAsync(pdfData, ct);

        // Store PDF URL as base64 data URI for now; Phase 8 will upload to SeaweedFS
        var fileUrl = $"data:application/pdf;base64,{Convert.ToBase64String(pdfBytes)}";

        po.Issue(fileUrl);
        _repo.Update(po);
        await _repo.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
