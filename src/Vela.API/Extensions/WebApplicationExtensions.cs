using AspNetCoreRateLimit;
using Scalar.AspNetCore;
using Vela.API.Hubs;

namespace Vela.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
        
        app.UseIpRateLimiting();
        
        app.UseCors("AllowFrontend");
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapHub<NotificationHub>("/hubs/notifications");
        app.MapControllers();
        
        return app;
    }
}