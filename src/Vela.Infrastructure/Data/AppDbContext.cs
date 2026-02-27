using Microsoft.EntityFrameworkCore;
using Vela.Domain.Entities;

namespace Vela.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<SwipeRecipe> SwipeRecipes => Set<SwipeRecipe>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // RecipeIngredient bruger sit eget Id som PK
        modelBuilder.Entity<RecipeIngredient>()
            .HasKey(ri => ri.Id);

        // Index for hurtig opslag på Recipe + Ingredient
        modelBuilder.Entity<RecipeIngredient>()
            .HasIndex(ri => new { ri.RecipeId, ri.IngredientId });

        modelBuilder.Entity<Ingredient>()
            .HasIndex(i => i.Name)
            .IsUnique();
        
        modelBuilder.Entity<SwipeRecipe>()
            .HasKey(s => s.SwipeId);

        modelBuilder.Entity<SwipeRecipe>()
            .HasIndex(s => new { s.UserId,  s.RecipeId })
            .IsUnique();
    }
}