namespace ProcureHub.Modules.DocumentManagement.Application.Services;

public interface IPdfGeneratorService
{
    Task<byte[]> GeneratePurchaseOrderPdfAsync(
        PurchaseOrderPdfData data,
        CancellationToken ct = default);
}

public record PurchaseOrderPdfData(
    string             PoNumber,
    string             CompanyName,
    string             VendorName,
    string             VendorAddress,
    string             Currency,
    DateTime           IssuedAt,
    DateTime?          ExpectedDelivery,
    string?            PaymentTerms,
    string?            DeliveryLocation,
    string?            Notes,
    decimal            TotalAmount,
    List<POItemPdfRow> Items
);

public record POItemPdfRow(
    string  Description,
    decimal Quantity,
    string  Uom,
    decimal UnitPrice,
    decimal TotalPrice
);
