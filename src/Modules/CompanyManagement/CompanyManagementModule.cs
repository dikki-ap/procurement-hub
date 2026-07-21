using Microsoft.Extensions.DependencyInjection;
using ProcureHub.Modules.CompanyManagement.Domain.Repositories;
using ProcureHub.Modules.CompanyManagement.Infrastructure.Repositories;

namespace ProcureHub.Modules.CompanyManagement;

public static class CompanyManagementModule
{
    public static IServiceCollection AddCompanyManagementServices(this IServiceCollection services)
    {
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        return services;
    }
}
