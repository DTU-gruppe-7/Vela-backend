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
    public DbSet<ShoppingList> ShoppingLists => Set<ShoppingList>();
    public DbSet<ShoppingListItem> ShoppingListItems => Set<ShoppingListItem>();
    public DbSet<MealPlan> MealPlans => Set<MealPlan>();
    public DbSet<MealPlanEntry> MealPlanEntries => Set<MealPlanEntry>();

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
        
        modelBuilder.Entity<ShoppingList>()
            .HasKey(sl => sl.Id);

        modelBuilder.Entity<ShoppingList>()
            .HasMany(sl => sl.Items)
            .WithOne(i => i.ShoppingList)
            .HasForeignKey(i => i.ShoppingListId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ShoppingListItem>()
            .HasKey(si => si.Id);

        modelBuilder.Entity<ShoppingListItem>()
            .HasOne(si => si.Ingredient)
            .WithMany()
            .HasForeignKey(si => si.IngredientId);

        // MealPlanEntry configuration
        modelBuilder.Entity<MealPlanEntry>()
            .HasKey(mpe => mpe.Id);

        modelBuilder.Entity<MealPlanEntry>()
            .HasOne(mpe => mpe.MealPlan)
            .WithMany(mp => mp.Entries)
            .HasForeignKey(mpe => mpe.MealPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MealPlanEntry>()
            .HasOne(mpe => mpe.Recipe)
            .WithMany()
            .HasForeignKey(mpe => mpe.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for hurtig opslag
        modelBuilder.Entity<MealPlanEntry>()
            .HasIndex(mpe => mpe.MealPlanId);

        modelBuilder.Entity<MealPlanEntry>()
            .HasIndex(mpe => new { mpe.MealPlanId, mpe.Day, mpe.MealType });
    }
}