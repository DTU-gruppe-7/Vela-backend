namespace Vela.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        
        app.UseCors("AllowFrontend");
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();
        
        return app;
    }
}