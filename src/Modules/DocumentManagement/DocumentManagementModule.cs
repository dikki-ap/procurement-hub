using Microsoft.Extensions.DependencyInjection;
using ProcureHub.Modules.DocumentManagement.Application.Options;
using ProcureHub.Modules.DocumentManagement.Application.Services;

namespace ProcureHub.Modules.DocumentManagement;

public static class DocumentManagementModule
{
    public static IServiceCollection AddDocumentManagementServices(
        this IServiceCollection services)
    {
        services.AddOptions<PdfOptions>()
            .BindConfiguration(PdfOptions.SectionName);

        services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();

        return services;
    }
}
