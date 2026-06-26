using Microsoft.OpenApi.Models;

namespace ProcureHub.API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = "ProcureHub API",
                Version     = "v1",
                Description = """
                    **Supplier Management & Procurement Workflow Portal**

                    ## Authentication
                    Uses Keycloak JWT Bearer tokens.
                    1. Obtain a token from Keycloak
                    2. Click **Authorize** and enter: `Bearer {token}`

                    ## Roles
                    | Role | Access Level |
                    |---|---|
                    | super_admin | Full access |
                    | purchasing | RFQ, PO, Evaluation |
                    | requester | Create Purchase Requisitions |
                    | approver | Approve / Revise / Reject |
                    | finance | Invoice, Payment |
                    | management | Read-only analytics |
                    | vendor_admin | Vendor profile and documents |
                    | vendor_staff | Submit quotations |

                    ## Timestamps
                    All timestamps are stored and returned as **UTC ISO 8601**.
                    """
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name         = "Authorization",
                Type         = SecuritySchemeType.Http,
                Scheme       = "Bearer",
                BearerFormat = "JWT",
                In           = ParameterLocation.Header,
                Description  = "JWT Bearer token from Keycloak"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme, Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
            foreach (var xml in xmlFiles)
                options.IncludeXmlComments(xml);
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(
        this IApplicationBuilder app,
        IWebHostEnvironment env)
    {
        if (env.IsProduction()) return app;

        app.UseSwagger();
        app.UseSwaggerUI(opts =>
        {
            opts.SwaggerEndpoint("/swagger/v1/swagger.json", "ProcureHub API v1");
            opts.RoutePrefix   = "swagger";
            opts.DocumentTitle = "ProcureHub API Documentation";
            opts.DisplayRequestDuration();
            opts.EnableFilter();
            opts.EnableDeepLinking();
            opts.ConfigObject.AdditionalItems["persistAuthorization"] = true;
        });

        return app;
    }
}
