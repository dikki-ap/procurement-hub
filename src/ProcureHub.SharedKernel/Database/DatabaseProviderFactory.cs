using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace ProcureHub.SharedKernel.Database;

public static class DatabaseProviderFactory
{
    public static DbContextOptionsBuilder Configure(
        DbContextOptionsBuilder options,
        IConfiguration configuration)
    {
        var providerStr = configuration["Database:Provider"] ?? "MariaDb";
        var provider    = Enum.Parse<DatabaseProvider>(providerStr, ignoreCase: true);

        var connectionStrings = configuration.GetSection("Database:ConnectionStrings");

        return provider switch
        {
            DatabaseProvider.MariaDb or DatabaseProvider.MySql => options
                .UseMySql(
                    connectionStrings[providerStr]!,
                    ServerVersion.AutoDetect(connectionStrings[providerStr]!),
                    o => o.SchemaBehavior(MySqlSchemaBehavior.Ignore))
                .UseSnakeCaseNamingConvention(),

            DatabaseProvider.PostgreSql => options
                .UseNpgsql(connectionStrings["PostgreSql"]!)
                .UseSnakeCaseNamingConvention(),

            _ => throw new NotSupportedException($"Database provider '{providerStr}' is not supported.")
        };
    }
}
