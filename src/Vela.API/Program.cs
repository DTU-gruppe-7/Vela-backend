using Microsoft.EntityFrameworkCore;
using Vela.API.Extensions;
using Vela.Infrastructure.Data;
using Vela.Application.Interfaces.External; // Husk denne til import-servicen

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPresentation(builder.Configuration)
    .AddInfrastructure(builder.Configuration);
    
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var dbContext = services.GetRequiredService<AppDbContext>();

    // RETRY LOGIK: Prøv 10 gange med 10 sekunders mellemrum
    int retries = 10;
    bool migrationSucceeded = false;

    while (retries > 0 && !migrationSucceeded)
    {
        try
        {
            logger.LogInformation("Attempting to run migrations... (Retries left: {Retries})", retries);
            await dbContext.Database.MigrateAsync();
            
            var importService = services.GetRequiredService<IRecipeImportService>();
            await importService.ImportRecipesFromJsonAsync();
            
            migrationSucceeded = true;
            logger.LogInformation("Database migration and seeding completed successfully.");
        }
        catch (Exception ex)
        {
            retries--;
            if (retries == 0)
            {
                logger.LogCritical(ex, "Could not connect to database after several attempts.");
                throw; // Stop appen hvis vi aldrig får forbindelse
            }
            logger.LogWarning("Database not ready yet. Waiting 10 seconds...");
            await Task.Delay(10000);
        }
    }
}
    
app.ConfigurePipeline();
app.Run();