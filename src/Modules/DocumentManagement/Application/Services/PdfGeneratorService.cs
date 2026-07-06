using Microsoft.Extensions.Options;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using ProcureHub.Modules.DocumentManagement.Application.Options;

namespace ProcureHub.Modules.DocumentManagement.Application.Services;

public class PdfGeneratorService : IPdfGeneratorService
{
    private readonly string _fontsPath;

    public PdfGeneratorService(IOptions<PdfOptions> options)
    {
        _fontsPath = options.Value.FontsPath;
        GlobalFontSettings.FontResolver = new ProcureHubFontResolver(_fontsPath);
    }

    public Task<byte[]> GeneratePurchaseOrderPdfAsync(
        PurchaseOrderPdfData data,
        CancellationToken ct = default)
    {
        using var document = new PdfDocument();
        document.Info.Title  = $"Purchase Order {data.PoNumber}";
        document.Info.Author = "ProcureHub";

        var page = document.AddPage();
        page.Size = PdfSharp.PageSize.A4;

        using var gfx = XGraphics.FromPdfPage(page);

        var fontTitle   = new XFont("Roboto", 16, XFontStyleEx.Bold);
        var fontBold    = new XFont("Roboto", 11, XFontStyleEx.Bold);
        var fontRegular = new XFont("Roboto", 10, XFontStyleEx.Regular);
        var fontSmall   = new XFont("Roboto",  8, XFontStyleEx.Regular);

        double left = 40, top = 40, width = page.Width - 80;
        double y = top;

        gfx.DrawString("PURCHASE ORDER", fontTitle, XBrushes.DarkBlue,
            new XRect(left, y, width, 30), XStringFormats.TopLeft);
        y += 30;

        gfx.DrawString(data.PoNumber, fontBold, XBrushes.Black, new XPoint(left, y));
        y += 25;

        gfx.DrawString($"From: {data.CompanyName}",  fontRegular, XBrushes.Black, new XPoint(left,       y));
        gfx.DrawString($"To:   {data.VendorName}",   fontRegular, XBrushes.Black, new XPoint(left + 300, y));
        y += 18;

        gfx.DrawString($"Date: {data.IssuedAt:dd MMMM yyyy}", fontRegular, XBrushes.Black, new XPoint(left, y));
        if (data.ExpectedDelivery.HasValue)
            gfx.DrawString($"Expected Delivery: {data.ExpectedDelivery:dd MMMM yyyy}",
                fontRegular, XBrushes.Black, new XPoint(left + 300, y));
        y += 18;

        if (!string.IsNullOrEmpty(data.DeliveryLocation))
        {
            gfx.DrawString($"Delivery: {data.DeliveryLocation}", fontSmall, XBrushes.Gray, new XPoint(left, y));
            y += 15;
        }

        if (!string.IsNullOrEmpty(data.PaymentTerms))
        {
            gfx.DrawString($"Payment: {data.PaymentTerms}", fontSmall, XBrushes.Gray, new XPoint(left, y));
            y += 15;
        }

        y += 10;

        var headers   = new[] { "No", "Description", "Qty", "Unit", "Unit Price", "Total" };
        var colWidths = new[] { 25f, 195f, 50f, 40f, 90f, 95f };

        DrawTableHeader(gfx, left, y, headers, colWidths, fontBold);
        y += 20;

        for (int i = 0; i < data.Items.Count; i++)
        {
            var item = data.Items[i];
            DrawTableRow(gfx, left, y, [
                (i + 1).ToString(),
                item.Description,
                item.Quantity.ToString("N2"),
                item.Uom,
                item.UnitPrice.ToString("N2"),
                item.TotalPrice.ToString("N2"),
            ], colWidths, fontRegular);
            y += 18;
        }

        y += 10;
        gfx.DrawString($"Total: {data.Currency} {data.TotalAmount:N2}",
            fontBold, XBrushes.Black, new XPoint(left + 300, y));

        if (!string.IsNullOrEmpty(data.Notes))
        {
            y += 25;
            gfx.DrawString("Notes:", fontBold, XBrushes.Black, new XPoint(left, y));
            y += 15;
            gfx.DrawString(data.Notes, fontSmall, XBrushes.Gray, new XPoint(left, y));
        }

        using var stream = new MemoryStream();
        document.Save(stream);
        return Task.FromResult(stream.ToArray());
    }

    private static void DrawTableHeader(
        XGraphics gfx, double x, double y,
        string[] headers, float[] widths, XFont font)
    {
        gfx.DrawRectangle(XBrushes.DarkBlue, new XRect(x, y, widths.Sum(), 18));
        double cx = x;
        foreach (var (h, w) in headers.Zip(widths))
        {
            gfx.DrawString(h, font, XBrushes.White,
                new XRect(cx + 2, y + 2, w - 4, 14), XStringFormats.TopLeft);
            cx += w;
        }
    }

    private static void DrawTableRow(
        XGraphics gfx, double x, double y,
        string[] cells, float[] widths, XFont font)
    {
        double cx = x;
        foreach (var (c, w) in cells.Zip(widths))
        {
            gfx.DrawString(c, font, XBrushes.Black,
                new XRect(cx + 2, y + 2, w - 4, 14), XStringFormats.TopLeft);
            cx += w;
        }
        gfx.DrawLine(XPens.LightGray, x, y + 17, x + widths.Sum(), y + 17);
    }
}

public class ProcureHubFontResolver : IFontResolver
{
    private readonly string _fontsPath;

    public ProcureHubFontResolver(string fontsPath) => _fontsPath = fontsPath;

    public string DefaultFontName => "Roboto";

    public byte[] GetFont(string faceName)
    {
        var fileName = faceName switch
        {
            "Roboto#b"  => "Roboto-Bold.ttf",
            "Roboto#bi" => "Roboto-Bold.ttf",
            "Roboto#i"  => "Roboto-Italic.ttf",
            _           => "Roboto-Regular.ttf"
        };
        return File.ReadAllBytes(Path.Combine(_fontsPath, fileName));
    }

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        => familyName.ToLower() == "roboto"
            ? new FontResolverInfo($"Roboto{(isBold ? "#b" : "")}{(isItalic ? "i" : "")}")
            : new FontResolverInfo("Roboto");
}
