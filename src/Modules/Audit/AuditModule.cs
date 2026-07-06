using Microsoft.Extensions.DependencyInjection;

namespace ProcureHub.Modules.Audit;

public static class AuditModule
{
    public static IServiceCollection AddAuditModule(this IServiceCollection services)
        => services; // Handlers registered globally via AddMediatR in Program.cs
}
