using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.External.MealDb;
using Vela.Infrastructure.Data;
using Vela.Infrastructure.External.MealDb;
using Vela.Infrastructure.Repositories;

namespace Vela.API.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        //Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("VelaDbConnection")));
        
        //Repositories
        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddScoped<IIngredientRepository, IngredientRepository>();
        
        //External Services
        services.AddHttpClient<IMealDbApiClient, MealDbApiClient>();
        services.AddScoped<IMeasureParser, MeasureParser>();
        services.AddScoped<IMealDbMapper, MealDbMapper>();
        services.AddScoped<IMealDbImportService, MealDbImportService>();
        
        return services;
    }
}