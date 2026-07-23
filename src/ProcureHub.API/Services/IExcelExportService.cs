namespace ProcureHub.API.Services;

public interface IExcelExportService
{
    Task<byte[]> ExportVendorsAsync(Guid companyId, CancellationToken ct = default);
    Task<byte[]> ExportPurchaseOrdersAsync(Guid companyId, CancellationToken ct = default);
    Task<byte[]> ExportSpendReportAsync(Guid companyId, DateOnly from, DateOnly to, CancellationToken ct = default);
}
