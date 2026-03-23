using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Vela.API.Notification;
using Vela.Application.Interfaces.Repository;
using Vela.Application.Interfaces.External;
using Vela.Application.Interfaces.Service;
using Vela.Application.Interfaces.Service.Notification;
using Vela.Application.Services;
using Vela.Application.Services.Notification;
using Vela.Infrastructure.Data;
using Vela.Infrastructure.Repositories;
using Vela.Infrastructure.External.RecipeImport;
using Vela.Infrastructure.Identity;

namespace Vela.API.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        //Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("VelaDbConnection")));
        
        // --- NYT: IDENTITY SETUP ---
        services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                // Her kan du evt. ændre password-krav senere
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        // --- NYT: JWT AUTHENTICATION SETUP ---
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false; // Kan sættes til true i produktion
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = configuration["JwtSettings:Audience"],
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"]!))
                };
                // SignalR sender ikke Authorization header over WebSocket — læs token fra query string
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/api/hubs/notifications"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        
        //Notifications
        services.AddScoped<IRealtimeNotificationService, SignalRNotificationService>();
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
        
        //Repositories
        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddScoped<IIngredientRepository, IngredientRepository>();
        services.AddScoped<IMealPlanRepository, MealPlanRepository>();
        services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
        services.AddScoped<ILikeRepository, LikeRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IGroupInviteRepository, GroupInviteRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        //Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<ILikeService, LikeService>();
        services.AddScoped<IShoppingListService, ShoppingListService>();
        services.AddScoped<IMealPlanService, MealPlanService>();
        services.AddScoped<IGroupAuthorizationService, GroupAuthorizationService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IGroupInviteService, GroupInviteService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Import Service
        services.AddScoped<IRecipeImportService, JsonRecipeImportService>();
        
        return services;
    }
}