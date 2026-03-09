using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Vela.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
            });

        services.AddEndpointsApiExplorer();
        
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                // 1. Opret Components og SecuritySchemes hvis de er null
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                // Tilføj Bearer skemaet
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Indtast dit JWT token."
                };

                // 2. NY SYNTAKS: Brug OpenApiSecuritySchemeReference
                var requirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer", document),
                        new List<string>()
                    }
                };

                // Anvend kravet globalt
                if (document.Paths != null)
                {
                    foreach (var path in document.Paths.Values)
                    {
                        foreach (var operation in path.Operations.Values)
                        {
                            operation.Security ??= new List<OpenApiSecurityRequirement>();
                            operation.Security.Add(requirement);
                        }
                    }
                }
                
                return Task.CompletedTask;
            });
        });

        // CORS opsætning
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                             ?? Array.Empty<string>();
        var allowedMethods = configuration.GetSection("Cors:AllowedMethods").Get<string[]>()
                             ?? new[] { "GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS" };
        var allowedHeaders = configuration.GetSection("Cors:AllowedHeaders").Get<string[]>()
                             ?? new[] { "Content-Type", "Authorization", "Accept" };
        
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .WithMethods(allowedMethods)
                        .WithHeaders(allowedHeaders)
                        .AllowCredentials();
                }
            });
        });
        
        return services;
    }
}