using Microsoft.Extensions.DependencyInjection;
using ProcureHub.Modules.ApprovalEngine.Domain.Repositories;
using ProcureHub.Modules.ApprovalEngine.Domain.Services;
using ProcureHub.Modules.ApprovalEngine.Infrastructure.Jobs;
using ProcureHub.Modules.ApprovalEngine.Infrastructure.Repositories;

namespace ProcureHub.Modules.ApprovalEngine;

public static class ApprovalEngineModule
{
    public static IServiceCollection AddApprovalEngineServices(this IServiceCollection services)
    {
        services.AddScoped<IApprovalWorkflowRepository, ApprovalWorkflowRepository>();
        services.AddScoped<IApprovalPolicyRepository,   ApprovalPolicyRepository>();
        services.AddScoped<ApprovalStateMachine>();
        services.AddScoped<ApprovalEscalationJob>();

        return services;
    }
}
