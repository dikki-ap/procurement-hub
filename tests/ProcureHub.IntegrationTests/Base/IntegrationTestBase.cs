using System.Text;
using System.Text.Json;
using ProcureHub.IntegrationTests.Infrastructure;

namespace ProcureHub.IntegrationTests.Base;

[Collection("Integration")]
public abstract class IntegrationTestBase : IClassFixture<TestWebApplicationFactory>
{
    protected readonly TestWebApplicationFactory Factory;

    protected static readonly Guid AdminUserId   = Guid.NewGuid();
    protected static readonly Guid CompanyId     = Guid.NewGuid();
    protected static readonly Guid Vendor1Id     = Guid.NewGuid();
    protected static readonly Guid Vendor2Id     = Guid.NewGuid();
    protected static readonly Guid Vendor3Id     = Guid.NewGuid();
    protected static readonly Guid ApproverUserId = Guid.NewGuid();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    protected IntegrationTestBase(TestWebApplicationFactory factory)
        => Factory = factory;

    protected HttpClient AsAdmin()
        => Factory.CreateClientWithRole(
            "super_admin", AdminUserId, "admin@test.com", "Admin User");

    protected HttpClient AsPurchasing(Guid? userId = null)
        => Factory.CreateClientWithRole(
            "purchasing", userId ?? Guid.NewGuid(), "purchasing@test.com", "Purchasing User");

    protected HttpClient AsApprover(Guid? userId = null)
        => Factory.CreateClientWithRole(
            "approver", userId ?? ApproverUserId, "approver@test.com", "Approver User");

    protected HttpClient AsFinance()
        => Factory.CreateClientWithRole(
            "finance", Guid.NewGuid(), "finance@test.com", "Finance User");

    protected HttpClient AsVendorAdmin(Guid? userId = null)
        => Factory.CreateClientWithRole(
            "vendor_admin", userId ?? Guid.NewGuid(), "vendor@test.com", "Vendor Admin");

    protected HttpClient AsRequester()
        => Factory.CreateClientWithRole(
            "requester", Guid.NewGuid(), "requester@test.com", "Requester User");

    protected static StringContent Json(object body)
        => new(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

    protected static async Task<T> ParseData<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        var doc  = JsonDocument.Parse(json);
        var data = doc.RootElement.GetProperty("data");
        return data.Deserialize<T>(JsonOptions)!;
    }

    protected static async Task<Guid> ParseGuidData(HttpResponseMessage response)
        => await ParseData<Guid>(response);
}

[CollectionDefinition("Integration")]
public class IntegrationCollectionDefinition : ICollectionFixture<TestWebApplicationFactory> { }
