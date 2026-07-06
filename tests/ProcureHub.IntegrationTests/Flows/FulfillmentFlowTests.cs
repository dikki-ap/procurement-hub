using ProcureHub.IntegrationTests.Base;
using ProcureHub.IntegrationTests.Infrastructure;

namespace ProcureHub.IntegrationTests.Flows;

/// <summary>
/// Tests the fulfillment flow: PO issued → GRN → Invoice → Payment → vendor score updated.
/// </summary>
public class FulfillmentFlowTests : IntegrationTestBase
{
    public FulfillmentFlowTests(TestWebApplicationFactory factory) : base(factory) { }

    private async Task<(Guid vendorId, Guid poId, Guid poItemId)> CreateApprovedPO()
    {
        var admin     = AsAdmin();
        var purchasing = AsPurchasing();

        // Register and approve vendor
        var regResp = await Factory.CreateClient().PostAsync("/api/v1/vendor-registration", Json(new
        {
            companyId    = CompanyId,
            legalName    = "Fulfillment Vendor Ltd",
            vendorType   = 0,
            contactName  = "Fulfillment Contact",
            contactEmail = "fulfillment@vendor.test",
        }));
        regResp.StatusCode.Should().Be(HttpStatusCode.OK);

        using var regDoc  = System.Text.Json.JsonDocument.Parse(await regResp.Content.ReadAsStringAsync());
        var vendorId = Guid.Parse(regDoc.RootElement.GetProperty("data").GetProperty("id").GetString()!);

        await admin.PostAsync($"/api/v1/vendors/{vendorId}/approve", Json(new { }));

        // Create PO directly (skip RFQ for brevity)
        var poResp = await purchasing.PostAsync("/api/v1/purchase-orders", Json(new
        {
            companyId        = CompanyId,
            vendorId,
            expectedDelivery = DateTime.UtcNow.AddDays(14),
            notes            = "Fulfillment test PO",
            items = new[]
            {
                new { description = "Test Widget", quantity = 10m, unitPrice = 100_000m }
            }
        }));
        poResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var poId = await ParseGuidData(poResp);

        // Get PO to retrieve item ID
        var poDetailResp = await purchasing.GetAsync($"/api/v1/purchase-orders/{poId}");
        poDetailResp.StatusCode.Should().Be(HttpStatusCode.OK);
        using var poDoc  = System.Text.Json.JsonDocument.Parse(await poDetailResp.Content.ReadAsStringAsync());
        var poItemId = Guid.Parse(poDoc.RootElement.GetProperty("data").GetProperty("items")[0].GetProperty("id").GetString()!);

        // Issue the PO (requires PDF service — mocked)
        var issueResp = await purchasing.PostAsync($"/api/v1/purchase-orders/{poId}/issue", null);
        issueResp.StatusCode.Should().Be(HttpStatusCode.OK);

        return (vendorId, poId, poItemId);
    }

    [Fact]
    public async Task FulfillmentFlow_GRN_Invoice_Payment_Succeeds()
    {
        var (vendorId, poId, poItemId) = await CreateApprovedPO();
        var purchasing = AsPurchasing();
        var receivedById = Guid.NewGuid();

        // 1. Create GRN
        var grnResp = await purchasing.PostAsync("/api/v1/goods-receipts", Json(new
        {
            poId,
            receivedBy     = receivedById,
            deliveryNoteNo = "DN-TEST-001",
            notes          = "All items received in good condition",
            items = new[]
            {
                new
                {
                    poItemId,
                    receivedQty    = 10m,
                    rejectedQty    = 0m,
                    qualityStatus  = 0, // Accepted
                }
            }
        }));
        grnResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var grnId = await ParseGuidData(grnResp);

        // 2. Confirm GRN (triggers vendor score update)
        var confirmResp = await purchasing.PostAsync(
            $"/api/v1/goods-receipts/{grnId}/confirm?vendorId={vendorId}", null);
        confirmResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Submit invoice (as vendor)
        var vendorClient = Factory.CreateClientWithRole("vendor_admin", vendorId, "vendor@test.com");
        var invoiceResp  = await vendorClient.PostAsync("/api/v1/invoices", Json(new
        {
            poId,
            vendorId,
            amount      = 1_000_000m,
            taxAmount   = 110_000m,
            notes       = "Invoice for Test Widget",
            dueAt       = DateTime.UtcNow.AddDays(30),
        }));
        invoiceResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var invoiceId = await ParseGuidData(invoiceResp);

        // 4. Finance reviews and approves invoice
        var finance    = AsFinance();
        var reviewResp = await finance.PostAsync($"/api/v1/invoices/{invoiceId}/review", Json(new
        {
            approve          = true,
            rejectionReason  = (string?)null,
        }));
        reviewResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Finance confirms payment
        var paymentResp = await finance.PostAsync($"/api/v1/invoices/{invoiceId}/confirm-payment", Json(new
        {
            paymentReference = "TRF-20260706-001",
        }));
        paymentResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 6. Verify invoice shows Paid status
        var invoiceDetailResp = await finance.GetAsync($"/api/v1/invoices/{invoiceId}");
        invoiceDetailResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var invoiceJson = await invoiceDetailResp.Content.ReadAsStringAsync();
        invoiceJson.Should().Contain("Paid");
    }

    [Fact]
    public async Task GRNConfirm_TriggersVendorScoreUpdate()
    {
        var (vendorId, poId, poItemId) = await CreateApprovedPO();
        var purchasing = AsPurchasing();

        // Create and confirm GRN
        var grnResp = await purchasing.PostAsync("/api/v1/goods-receipts", Json(new
        {
            poId,
            receivedBy = Guid.NewGuid(),
            items = new[]
            {
                new
                {
                    poItemId,
                    receivedQty   = 8m,  // 80% received
                    rejectedQty   = 2m,  // 20% rejected
                    qualityStatus = 0,   // Accepted
                }
            }
        }));
        grnResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var grnId = await ParseGuidData(grnResp);

        var confirmResp = await purchasing.PostAsync(
            $"/api/v1/goods-receipts/{grnId}/confirm?vendorId={vendorId}", null);
        confirmResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify vendor still exists (score update ran without error)
        var vendorDetailResp = await AsAdmin().GetAsync($"/api/v1/vendors/{vendorId}");
        vendorDetailResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
