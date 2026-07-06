using ProcureHub.IntegrationTests.Base;
using ProcureHub.IntegrationTests.Infrastructure;

namespace ProcureHub.IntegrationTests.Security;

public class SecurityTests : IntegrationTestBase
{
    public SecurityTests(TestWebApplicationFactory factory) : base(factory) { }

    // ── Unauthenticated → 401 ────────────────────────────────────────────────

    [Fact]
    public async Task Unauthenticated_GetVendors_Returns401()
    {
        var client   = Factory.CreateClient();
        var response = await client.GetAsync($"/api/v1/vendors?companyId={CompanyId}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Unauthenticated_GetAuditLog_Returns401()
    {
        var client   = Factory.CreateClient();
        var response = await client.GetAsync("/api/v1/audit");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Unauthenticated_CreatePR_Returns401()
    {
        var client   = Factory.CreateClient();
        var response = await client.PostAsync("/api/v1/purchase-requisitions", Json(new { }));
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Wrong role → 403 ────────────────────────────────────────────────────

    [Fact]
    public async Task Requester_GetAuditLog_Returns403()
    {
        var client   = AsRequester();
        var response = await client.GetAsync("/api/v1/audit");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Requester_ApproveVendor_Returns403()
    {
        var client   = AsRequester();
        var response = await client.PostAsync($"/api/v1/vendors/{Guid.NewGuid()}/approve", Json(new { }));
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task VendorAdmin_AccessInternalAuditLog_Returns403()
    {
        var client   = Factory.CreateClientWithRole("vendor_admin", Guid.NewGuid());
        var response = await client.GetAsync("/api/v1/audit");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task VendorAdmin_CreateRFQ_Returns403()
    {
        var client   = Factory.CreateClientWithRole("vendor_admin", Guid.NewGuid());
        var response = await client.PostAsync("/api/v1/rfqs", Json(new { }));
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── Public endpoints ─────────────────────────────────────────────────────

    [Fact]
    public async Task VendorRegistration_IsPublic_Returns200OrValidationError()
    {
        var client   = Factory.CreateClient();
        var response = await client.PostAsync("/api/v1/vendor-registration", Json(new
        {
            companyId    = CompanyId,
            legalName    = "Integration Test Corp",
            vendorType   = 0, // Manufacturer
            contactName  = "John Doe",
            contactEmail = "john@integration.test",
        }));
        // 200 = success, 400 = validation error (either is fine — endpoint is accessible)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }
}
