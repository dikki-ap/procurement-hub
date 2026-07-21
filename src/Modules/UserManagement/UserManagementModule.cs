using Microsoft.Extensions.DependencyInjection;
using ProcureHub.Modules.UserManagement.Domain.Repositories;
using ProcureHub.Modules.UserManagement.Infrastructure.Repositories;

namespace ProcureHub.Modules.UserManagement;

public static class UserManagementModule
{
    public static IServiceCollection AddUserManagementServices(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}
