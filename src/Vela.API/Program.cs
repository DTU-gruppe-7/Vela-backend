using Microsoft.EntityFrameworkCore;
using Vela.API.Extensions;
using Vela.Application.Interfaces.External;
using Vela.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPresentation(builder.Configuration)
    .AddInfrastructure(builder.Configuration);
    
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        // 1. Kør database migration først
        var dbContext = services.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("Database migration completed.");

        // 2. Kør din import service
        var importService = services.GetRequiredService<IRecipeImportService>();
        await importService.ImportRecipesFromJsonAsync();
        Console.WriteLine("Recipes imported successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred during startup: {ex.Message}");
    }
}
    
app.ConfigurePipeline();
    
app.Run();