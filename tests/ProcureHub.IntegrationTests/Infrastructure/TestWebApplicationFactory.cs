using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using ProcureHub.Modules.DocumentManagement.Application.Services;
using ProcureHub.SharedKernel.Abstractions;
using Testcontainers.MariaDb;

namespace ProcureHub.IntegrationTests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MariaDbContainer _mariaDb = new MariaDbBuilder()
        .WithImage("mariadb:11.4")
        .WithDatabase("procurehub_test")
        .WithUsername("test")
        .WithPassword("test")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(3306))
        .Build();

    public Mock<IEmailService>        MockEmail        { get; } = new();
    public Mock<INotificationService> MockNotification { get; } = new();
    public Mock<IStorageService>      MockStorage      { get; } = new();
    public Mock<IPdfGeneratorService> MockPdf          { get; } = new();

    public async Task InitializeAsync()
    {
        await _mariaDb.StartAsync();

        MockStorage
            .Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.test/file.pdf");

        MockStorage
            .Setup(s => s.GetPresignedUrlAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://storage.test/presigned");

        MockStorage
            .Setup(s => s.EnsureBucketExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockStorage
            .Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockPdf
            .Setup(p => p.GeneratePurchaseOrderPdfAsync(It.IsAny<PurchaseOrderPdfData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // PDF magic bytes
    }

    public new async Task DisposeAsync()
    {
        await _mariaDb.StopAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connString = _mariaDb.GetConnectionString();

        builder.UseSetting("Database:Provider", "MariaDb");
        builder.UseSetting("Database:ConnectionStrings:MariaDb", connString);

        // Disable Serilog file sink
        builder.UseSetting("Serilog:WriteTo:1:Name", "Console");

        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Replace real auth with TestAuthHandler
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<TestAuthOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });

            // Replace external services with mocks
            services.AddScoped<IEmailService>(_ => MockEmail.Object);
            services.AddScoped<INotificationService>(_ => MockNotification.Object);
            services.AddScoped<IStorageService>(_ => MockStorage.Object);
            services.AddScoped<IPdfGeneratorService>(_ => MockPdf.Object);

            // Remove the real Hangfire server to avoid background job interference
            var hangfireServerDescriptor = services
                .FirstOrDefault(d => d.ServiceType.Name.Contains("IBackgroundJobServer"));
            if (hangfireServerDescriptor is not null)
                services.Remove(hangfireServerDescriptor);
        });
    }

    public HttpClient CreateClientWithRole(
        string role,
        Guid?  userId = null,
        string email  = "test@test.com",
        string name   = "Test User")
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role",   role);
        client.DefaultRequestHeaders.Add("X-Test-Email",  email);
        client.DefaultRequestHeaders.Add("X-Test-Name",   name);

        if (userId.HasValue)
            client.DefaultRequestHeaders.Add("X-Test-UserId", userId.Value.ToString());

        return client;
    }
}
