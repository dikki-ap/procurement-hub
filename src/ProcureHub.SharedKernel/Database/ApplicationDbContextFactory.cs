using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using ProcureHub.SharedKernel.Abstractions;

namespace ProcureHub.SharedKernel.Database;

/// <summary>
/// Design-time factory for EF Core tooling (migrations add/remove).
/// Bypasses the full application host so migrations can be generated
/// without a live database or running services.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // EF tools CWD may be the startup-project dir or the solution root.
        // Walk up until we find the solution root (contains the sln file).
        var apiProjectDir = LocateApiProjectDir();

        var config = new ConfigurationBuilder()
            .SetBasePath(apiProjectDir)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        DatabaseProviderFactory.Configure(optionsBuilder, config);

        return new ApplicationDbContext(
            optionsBuilder.Options,
            new DesignTimeCurrentUserService(),
            new Microsoft.AspNetCore.Http.HttpContextAccessor());
    }

    private static string LocateApiProjectDir()
    {
        var dir = Directory.GetCurrentDirectory();

        // If we're already inside the API project directory
        if (File.Exists(Path.Combine(dir, "appsettings.json")))
            return dir;

        // Walk up looking for solution root, then find API project under it
        var current = new DirectoryInfo(dir);
        while (current != null)
        {
            if (current.GetFiles("*.sln").Length > 0)
            {
                var apiDir = Path.Combine(current.FullName, "src", "ProcureHub.API");
                if (Directory.Exists(apiDir))
                    return apiDir;
            }
            current = current.Parent;
        }

        return dir;
    }

    private sealed class DesignTimeCurrentUserService : ICurrentUserService
    {
        public Guid?  UserId        => null;
        public Guid?  VendorUserId  => null;
        public string? KeycloakId   => null;
        public string? Email        => null;
        public string? FullName     => null;
        public string? IpAddress    => null;
        public string? UserAgent    => null;
        public IReadOnlyList<string> Roles => [];
        public bool IsAuthenticated => false;
        public bool HasRole(string role)              => false;
        public bool HasAnyRole(params string[] roles) => false;
    }
}
