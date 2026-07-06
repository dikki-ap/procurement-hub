using ProcureHub.IntegrationTests.Base;
using ProcureHub.IntegrationTests.Infrastructure;

namespace ProcureHub.IntegrationTests.Registration;

public class VendorRegistrationTests : IntegrationTestBase
{
    public VendorRegistrationTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_ValidPayload_Returns200WithVendorId()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync("/api/v1/vendor-registration", Json(new
        {
            companyId    = CompanyId,
            legalName    = "New Vendor Corp",
            tradeName    = "NVC",
            vendorType   = 0, // Manufacturer
            npwp         = "12.345.678.9-012.000",
            contactName  = "Jane Smith",
            contactEmail = "jane@newvendor.test",
            contactPhone = "+62812345678",
        }));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json  = await response.Content.ReadAsStringAsync();
        json.Should().Contain("id");
    }

    [Fact]
    public async Task Register_ThenAdminApproves_VendorStatusBecomesActive()
    {
        // 1. Register vendor
        var registerResp = await Factory.CreateClient()
            .PostAsync("/api/v1/vendor-registration", Json(new
            {
                companyId    = CompanyId,
                legalName    = "Approvable Vendor Ltd",
                vendorType   = 1, // Distributor
                contactName  = "Bob",
                contactEmail = "bob@approvable.test",
            }));

        registerResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var registrationData = await parseRegistrationResult(registerResp);
        var vendorId = registrationData;

        // 2. Admin lists vendors — new vendor should be in Pending state
        var admin     = AsAdmin();
        var listResp  = await admin.GetAsync($"/api/v1/vendors?companyId={CompanyId}");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Admin approves the vendor
        var approveResp = await admin.PostAsync(
            $"/api/v1/vendors/{vendorId}/approve", Json(new { }));
        approveResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Get vendor — status should now be Active
        var detailResp = await admin.GetAsync($"/api/v1/vendors/{vendorId}");
        detailResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await detailResp.Content.ReadAsStringAsync();
        json.Should().Contain("Active");
    }

    private static async Task<Guid> parseRegistrationResult(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var idStr = doc.RootElement
            .GetProperty("data")
            .GetProperty("id")
            .GetString()!;
        return Guid.Parse(idStr);
    }
}
