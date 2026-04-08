using System.Text.Json.Serialization;
using AspNetCoreRateLimit;
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
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Indtast dit JWT token."
                };

                var requirement = new OpenApiSecurityRequirement
                {
                    { new OpenApiSecuritySchemeReference("Bearer", document), new List<string>() }
                };

                foreach (var path in document.Paths.Values)
                {
                    if (path.Operations is null)
                        continue;

                    foreach (var operation in path.Operations.Values)
                    {
                        operation.Security ??= new List<OpenApiSecurityRequirement>();
                        operation.Security.Add(requirement);
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
        
        // --- RATE LIMITING OPSÆTNING ---
        // 1. Tilføj Memory Cache (nødvendigt for at gemme anmodningstællere)
        services.AddMemoryCache();

        // 2. Indlæs konfigurationen fra appsettings.json
        services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));

        // 3. Tilføj in-memory lagring af tællere og regler
        services.AddInMemoryRateLimiting();

        // 4. Registrer standard konfigurationen for rate limiting
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        
        //Notifications
        services.AddSignalR()
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
            });
        
        return services;
    }
}