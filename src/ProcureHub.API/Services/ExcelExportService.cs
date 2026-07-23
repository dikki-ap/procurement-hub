using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ProcureHub.Modules.MasterData.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Entities;
using ProcureHub.Modules.Procurement.Domain.Enums;
using ProcureHub.Modules.VendorManagement.Domain.Entities;
using ProcureHub.SharedKernel.Database;

namespace ProcureHub.API.Services;

public class ExcelExportService : IExcelExportService
{
    private readonly ApplicationDbContext _db;

    public ExcelExportService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<byte[]> ExportVendorsAsync(Guid companyId, CancellationToken ct = default)
    {
        var vendors = await _db.Set<Vendor>()
            .Where(v => v.CompanyId == companyId)
            .Include(v => v.Contacts)
            .OrderBy(v => v.VendorCode)
            .ToListAsync(ct);

        var currencies = await _db.Set<Currency>()
            .Where(c => c.IsActive)
            .ToListAsync(ct);

        var paymentTerms = await _db.Set<PaymentTerm>()
            .Where(p => p.CompanyId == companyId && p.IsActive)
            .ToListAsync(ct);

        var currencyMap    = currencies.ToDictionary(c => c.Id, c => c.Code);
        var paymentTermMap = paymentTerms.ToDictionary(p => p.Id, p => p.Name);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Vendors");

        ApplyHeaders(ws, [
            "Code", "Legal Name", "Trade Name", "Type", "Status", "Tier", "Score",
            "Is PKP", "PPh Rate (%)", "Default Currency", "Default Payment Term",
            "Address", "City", "Province", "Postal Code", "Country",
            "Contact Name", "Contact Email", "Contact Phone",
            "NPWP", "SIUP", "NIB",
            "Approved Date", "Created Date",
        ]);

        int row = 2;
        foreach (var v in vendors)
        {
            var contact = v.Contacts.FirstOrDefault();
            ws.Cell(row, 1).Value  = v.VendorCode;
            ws.Cell(row, 2).Value  = v.LegalName;
            ws.Cell(row, 3).Value  = v.TradeName ?? "";
            ws.Cell(row, 4).Value  = v.VendorType.ToString();
            ws.Cell(row, 5).Value  = v.Status.ToString();
            ws.Cell(row, 6).Value  = v.Tier.ToString();
            ws.Cell(row, 7).Value  = (double)v.Score;
            ws.Cell(row, 8).Value  = v.IsPkp ? "Yes" : "No";
            ws.Cell(row, 9).Value  = v.PphRate.HasValue ? (double)v.PphRate.Value : 0d;
            ws.Cell(row, 10).Value = v.DefaultCurrencyId.HasValue && currencyMap.TryGetValue(v.DefaultCurrencyId.Value, out var curr) ? curr : "";
            ws.Cell(row, 11).Value = v.DefaultPaymentTermId.HasValue && paymentTermMap.TryGetValue(v.DefaultPaymentTermId.Value, out var pt) ? pt : "";
            ws.Cell(row, 12).Value = v.Address ?? "";
            ws.Cell(row, 13).Value = v.City ?? "";
            ws.Cell(row, 14).Value = v.Province ?? "";
            ws.Cell(row, 15).Value = v.PostalCode ?? "";
            ws.Cell(row, 16).Value = v.Country ?? "";
            ws.Cell(row, 17).Value = contact?.Name ?? "";
            ws.Cell(row, 18).Value = contact?.Email ?? "";
            ws.Cell(row, 19).Value = contact?.Phone ?? "";
            ws.Cell(row, 20).Value = v.Npwp ?? "";
            ws.Cell(row, 21).Value = v.Siup ?? "";
            ws.Cell(row, 22).Value = v.Nib ?? "";
            ws.Cell(row, 23).Value = v.ApprovedAt.HasValue ? v.ApprovedAt.Value.ToString("yyyy-MM-dd") : "";
            ws.Cell(row, 24).Value = v.CreatedAt.ToString("yyyy-MM-dd");
            row++;
        }

        ws.Columns().AdjustToContents();
        return ToBytes(wb);
    }

    public async Task<byte[]> ExportPurchaseOrdersAsync(Guid companyId, CancellationToken ct = default)
    {
        var pos = await _db.Set<PurchaseOrder>()
            .Where(p => p.CompanyId == companyId)
            .Include(p => p.Vendor)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        var currencies   = await _db.Set<Currency>().ToListAsync(ct);
        var paymentTerms = await _db.Set<PaymentTerm>()
            .Where(p => p.CompanyId == companyId)
            .ToListAsync(ct);

        var currencyMap    = currencies.ToDictionary(c => c.Id, c => c.Code);
        var paymentTermMap = paymentTerms.ToDictionary(p => p.Id, p => p.Name);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Purchase Orders");

        ApplyHeaders(ws, [
            "PO Number", "Vendor", "Status", "Total Amount", "Currency",
            "Payment Term", "Issued Date", "Expected Delivery", "Actual Delivery",
            "Acknowledged Date", "Completed Date", "Created Date",
        ]);

        int row = 2;
        foreach (var po in pos)
        {
            ws.Cell(row, 1).Value  = po.PONumber;
            ws.Cell(row, 2).Value  = po.Vendor?.LegalName ?? "";
            ws.Cell(row, 3).Value  = po.Status.ToString();
            ws.Cell(row, 4).Value  = (double)po.TotalAmount;
            ws.Cell(row, 5).Value  = po.CurrencyId.HasValue && currencyMap.TryGetValue(po.CurrencyId.Value, out var curr) ? curr : "IDR";
            ws.Cell(row, 6).Value  = po.PaymentTermId.HasValue && paymentTermMap.TryGetValue(po.PaymentTermId.Value, out var pt) ? pt : "";
            ws.Cell(row, 7).Value  = po.IssuedAt.HasValue          ? po.IssuedAt.Value.ToString("yyyy-MM-dd")          : "";
            ws.Cell(row, 8).Value  = po.ExpectedDelivery.HasValue   ? po.ExpectedDelivery.Value.ToString("yyyy-MM-dd")  : "";
            ws.Cell(row, 9).Value  = po.ActualDelivery.HasValue     ? po.ActualDelivery.Value.ToString("yyyy-MM-dd")    : "";
            ws.Cell(row, 10).Value = po.AcknowledgedAt.HasValue     ? po.AcknowledgedAt.Value.ToString("yyyy-MM-dd")   : "";
            ws.Cell(row, 11).Value = po.CompletedAt.HasValue        ? po.CompletedAt.Value.ToString("yyyy-MM-dd")       : "";
            ws.Cell(row, 12).Value = po.CreatedAt.ToString("yyyy-MM-dd");
            row++;
        }

        ws.Column(4).Style.NumberFormat.Format = "#,##0.00";
        ws.Columns().AdjustToContents();
        return ToBytes(wb);
    }

    public async Task<byte[]> ExportSpendReportAsync(
        Guid companyId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var fromDt = from.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toDt   = to.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var paidInvoices = await _db.Set<Invoice>()
            .Where(i =>
                i.Status == InvoiceStatus.Paid &&
                i.PaidAt >= fromDt &&
                i.PaidAt <= toDt &&
                i.PurchaseOrder != null &&
                i.PurchaseOrder.CompanyId == companyId)
            .Include(i => i.PurchaseOrder)
            .ToListAsync(ct);

        var poIds = paidInvoices.Select(i => i.POId).Distinct().ToList();

        var poItems = await _db.Set<POItem>()
            .Where(item => poIds.Contains(item.POId))
            .ToListAsync(ct);

        var materialCategoryMap = new Dictionary<Guid, string>();
        if (poItems.Any(i => i.MaterialId.HasValue))
        {
            var materialIds = poItems
                .Where(i => i.MaterialId.HasValue)
                .Select(i => i.MaterialId!.Value)
                .Distinct()
                .ToList();

            var materials = await _db.Set<Material>()
                .Where(m => materialIds.Contains(m.Id))
                .Include(m => m.Category)
                .ToListAsync(ct);

            foreach (var m in materials)
                if (m.Category is not null)
                    materialCategoryMap[m.Id] = m.Category.Name;
        }

        using var wb = new XLWorkbook();

        // ── Sheet 1: Monthly Spend ───────────────────────────────────────────
        var wsMonthly = wb.Worksheets.Add("Monthly Spend");
        ApplyHeaders(wsMonthly, ["Month", "Paid Invoices", "Total Amount"]);

        var monthly = paidInvoices
            .GroupBy(i => new { i.PaidAt!.Value.Year, i.PaidAt!.Value.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                Count = g.Count(),
                Total = g.Sum(i => i.TotalAmount),
            })
            .ToList();

        int mRow = 2;
        foreach (var m in monthly)
        {
            wsMonthly.Cell(mRow, 1).Value = m.Month;
            wsMonthly.Cell(mRow, 2).Value = m.Count;
            wsMonthly.Cell(mRow, 3).Value = (double)m.Total;
            mRow++;
        }

        wsMonthly.Column(3).Style.NumberFormat.Format = "#,##0.00";
        wsMonthly.Columns().AdjustToContents();

        // ── Sheet 2: Spend by Category ───────────────────────────────────────
        var wsCategory = wb.Worksheets.Add("Spend by Category");
        ApplyHeaders(wsCategory, ["Category", "PO Items", "Total Spend"]);

        var invoicePOIdSet = paidInvoices.Select(i => i.POId).ToHashSet();
        var categorySpend = poItems
            .Where(item => invoicePOIdSet.Contains(item.POId))
            .GroupBy(item =>
                item.MaterialId.HasValue && materialCategoryMap.TryGetValue(item.MaterialId.Value, out var cat)
                    ? cat
                    : "Uncategorized")
            .OrderByDescending(g => g.Sum(i => i.TotalPrice))
            .Select(g => new
            {
                Category = g.Key,
                Count    = g.Count(),
                Total    = g.Sum(i => i.TotalPrice),
            })
            .ToList();

        int cRow = 2;
        foreach (var c in categorySpend)
        {
            wsCategory.Cell(cRow, 1).Value = c.Category;
            wsCategory.Cell(cRow, 2).Value = c.Count;
            wsCategory.Cell(cRow, 3).Value = (double)c.Total;
            cRow++;
        }

        wsCategory.Column(3).Style.NumberFormat.Format = "#,##0.00";
        wsCategory.Columns().AdjustToContents();

        // ── Sheet 3: Invoice Details ─────────────────────────────────────────
        var wsInvoices = wb.Worksheets.Add("Invoice Details");
        ApplyHeaders(wsInvoices, [
            "Invoice Number", "PO Number", "Amount", "Tax (PPN)",
            "PPh Withholding", "Total", "Net Payable", "Paid Date",
        ]);

        int iRow = 2;
        foreach (var inv in paidInvoices.OrderBy(i => i.PaidAt))
        {
            wsInvoices.Cell(iRow, 1).Value = inv.InvoiceNumber;
            wsInvoices.Cell(iRow, 2).Value = inv.PurchaseOrder?.PONumber ?? "";
            wsInvoices.Cell(iRow, 3).Value = (double)inv.Amount;
            wsInvoices.Cell(iRow, 4).Value = (double)inv.TaxAmount;
            wsInvoices.Cell(iRow, 5).Value = (double)inv.WithholdingTax;
            wsInvoices.Cell(iRow, 6).Value = (double)inv.TotalAmount;
            wsInvoices.Cell(iRow, 7).Value = (double)inv.NetPayable;
            wsInvoices.Cell(iRow, 8).Value = inv.PaidAt.HasValue ? inv.PaidAt.Value.ToString("yyyy-MM-dd") : "";
            iRow++;
        }

        for (int col = 3; col <= 7; col++)
            wsInvoices.Column(col).Style.NumberFormat.Format = "#,##0.00";

        wsInvoices.Columns().AdjustToContents();

        return ToBytes(wb);
    }

    private static void ApplyHeaders(IXLWorksheet ws, string[] headers)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold             = true;
            cell.Style.Font.FontColor        = XLColor.White;
            cell.Style.Fill.BackgroundColor  = XLColor.FromHtml("#1e3a5f");
        }
        ws.SheetView.FreezeRows(1);
    }

    private static byte[] ToBytes(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
