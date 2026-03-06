using System.Text.Json.Serialization;

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
        services.AddSwaggerGen();
        services.AddOpenApi();
        
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