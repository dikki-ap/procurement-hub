using ProcureHub.IntegrationTests.Base;
using ProcureHub.IntegrationTests.Infrastructure;

namespace ProcureHub.IntegrationTests.Flows;

/// <summary>
/// Tests the full procurement flow: PR → RFQ (3 vendors) → 3 bids → evaluate → award → PO.
/// </summary>
public class ProcurementFlowTests : IntegrationTestBase
{
    public ProcurementFlowTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task ProcurementFlow_PR_To_PO_SuccessfullyCreated()
    {
        var purchasing = AsPurchasing();

        // 1. Create PR
        var prResp = await purchasing.PostAsync("/api/v1/purchase-requisitions", Json(new
        {
            companyId        = CompanyId,
            title            = "Procurement Flow Test PR",
            description      = "End-to-end test",
            department       = "IT",
            deliveryLocation = "Warehouse A",
            requiredDate     = DateTime.UtcNow.AddDays(60),
            items = new[]
            {
                new
                {
                    itemDescription    = "Industrial Pump",
                    quantity           = 2m,
                    estimatedUnitPrice = 5_000_000m,
                    unitLabel          = "unit",
                }
            }
        }));
        prResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var prId = await ParseGuidData(prResp);

        // 2. Submit PR
        var submitPrResp = await purchasing.PostAsync(
            $"/api/v1/purchase-requisitions/{prId}/submit", null);
        submitPrResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Create RFQ
        var rfqResp = await purchasing.PostAsync("/api/v1/rfqs", Json(new
        {
            companyId             = CompanyId,
            title                 = "RFQ for Industrial Pump",
            purchaseRequisitionId = prId,
            bidDeadline           = DateTime.UtcNow.AddDays(7),
            deliveryDate          = DateTime.UtcNow.AddDays(30),
            notes                 = "Urgent",
            items = new[]
            {
                new
                {
                    itemDescription = "Industrial Pump",
                    quantity        = 2m,
                    unitLabel       = "unit",
                }
            }
        }));
        rfqResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var rfqId = await ParseGuidData(rfqResp);

        // 4. Register 3 vendors
        var v1Id = await RegisterVendor("Vendor Alpha", "alpha@test.com");
        var v2Id = await RegisterVendor("Vendor Beta",  "beta@test.com");
        var v3Id = await RegisterVendor("Vendor Gamma", "gamma@test.com");

        // 5. Invite all 3 vendors to the RFQ
        var inviteResp = await purchasing.PostAsync(
            $"/api/v1/rfqs/{rfqId}/invite-vendors",
            Json(new[] { v1Id, v2Id, v3Id }));
        inviteResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 6. Open RFQ (requires ≥3 vendors)
        var openResp = await purchasing.PostAsync($"/api/v1/rfqs/{rfqId}/open", null);
        openResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 7. Get RFQ detail to retrieve item IDs
        var rfqDetailResp = await purchasing.GetAsync($"/api/v1/rfqs/{rfqId}");
        rfqDetailResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var rfqJson = await rfqDetailResp.Content.ReadAsStringAsync();

        // 8. Each vendor submits a bid (via vendor_admin role)
        var bid1Id = await SubmitBid(rfqId, v1Id, 4_800_000m);
        var bid2Id = await SubmitBid(rfqId, v2Id, 4_500_000m);
        var bid3Id = await SubmitBid(rfqId, v3Id, 4_600_000m);

        // 9. Close RFQ
        var closeResp = await purchasing.PostAsync($"/api/v1/rfqs/{rfqId}/close", null);
        closeResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 10. Evaluate bids
        var evaluateResp = await purchasing.PostAsync($"/api/v1/rfqs/{rfqId}/evaluate", Json(new
        {
            rfqId         = rfqId,
            priceWeight   = 0.5m,
            qualityWeight = 0.3m,
            deliveryWeight = 0.2m,
            scores = new[]
            {
                new { quotationId = bid1Id, vendorId = v1Id, qualityScore = 80m, deliveryScore = 85m },
                new { quotationId = bid2Id, vendorId = v2Id, qualityScore = 90m, deliveryScore = 90m },
                new { quotationId = bid3Id, vendorId = v3Id, qualityScore = 75m, deliveryScore = 80m },
            }
        }));
        evaluateResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 11. Award to vendor 2 (best overall score)
        var awardResp = await purchasing.PostAsync($"/api/v1/rfqs/{rfqId}/award", Json(new
        {
            rfqId       = rfqId,
            quotationId = bid2Id,
            vendorId    = v2Id,
        }));
        awardResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 12. Create PO
        var poResp = await purchasing.PostAsync("/api/v1/purchase-orders", Json(new
        {
            companyId        = CompanyId,
            vendorId         = v2Id,
            rfqId            = rfqId,
            expectedDelivery = DateTime.UtcNow.AddDays(30),
            notes            = "Please deliver to Warehouse A",
            items = new[]
            {
                new
                {
                    description = "Industrial Pump",
                    quantity    = 2m,
                    unitPrice   = 4_500_000m,
                }
            }
        }));
        poResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var poId = await ParseGuidData(poResp);

        poId.Should().NotBeEmpty();
    }

    private async Task<Guid> RegisterVendor(string name, string email)
    {
        var resp = await Factory.CreateClient().PostAsync("/api/v1/vendor-registration", Json(new
        {
            companyId    = CompanyId,
            legalName    = name,
            vendorType   = 0, // Manufacturer
            contactName  = name,
            contactEmail = email,
        }));

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        using var doc  = System.Text.Json.JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var       idStr = doc.RootElement.GetProperty("data").GetProperty("id").GetString()!;
        var vendorId   = Guid.Parse(idStr);

        // Admin approves the vendor so it's active for bidding
        var approveResp = await AsAdmin().PostAsync(
            $"/api/v1/vendors/{vendorId}/approve", Json(new { }));
        approveResp.IsSuccessStatusCode.Should().BeTrue();

        return vendorId;
    }

    private async Task<Guid> SubmitBid(Guid rfqId, Guid vendorId, decimal price)
    {
        // Get RFQ items first
        var rfqResp = await AsPurchasing().GetAsync($"/api/v1/rfqs/{rfqId}");
        var rfqJson = await rfqResp.Content.ReadAsStringAsync();

        using var doc    = System.Text.Json.JsonDocument.Parse(rfqJson);
        var rfqItems     = doc.RootElement.GetProperty("data").GetProperty("items");
        var rfqItemId    = Guid.Parse(rfqItems[0].GetProperty("id").GetString()!);

        var vendorClient = Factory.CreateClientWithRole("vendor_admin", vendorId, $"vendor+{vendorId}@test.com");
        var resp = await vendorClient.PostAsync("/api/v1/quotations", Json(new
        {
            rfqId,
            vendorId,
            notes = "Our best offer",
            items = new[]
            {
                new { rfqItemId, unitPrice = price }
            }
        }));

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        return await ParseGuidData(resp);
    }
}
