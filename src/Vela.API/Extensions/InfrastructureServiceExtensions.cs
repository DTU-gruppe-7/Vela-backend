using Microsoft.EntityFrameworkCore;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.External;
using Vela.Application.Interfaces.Service;
using Vela.Application.Services;
using Vela.Infrastructure.Data;
using Vela.Infrastructure.Repositories;
using Vela.Infrastructure.External.RecipeImport;

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
        services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
        
        //Services
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<ISwipeService, SwipeService>();
        services.AddScoped<IShoppingListService, ShoppingListService>();
        // Import Service
        services.AddScoped<IRecipeImportService, JsonRecipeImportService>();
        // Swipe Repository
        services.AddScoped<ISwipeRepository, SwipeRepository>();
        
        return services;
    }
}