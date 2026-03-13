using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vela.Domain.Entities;
using Vela.Infrastructure.Identity;

namespace Vela.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser>
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
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<GroupInvite> GroupInvites => Set<GroupInvite>();
    public DbSet<Match> Matches => Set<Match>();

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
            .WithOne()
            .HasForeignKey(i => i.ShoppingListId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ShoppingListItem>()
            .HasKey(si => si.Id);

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
            .HasIndex(mpe => new { mpe.MealPlanId, mpe.Date, mpe.MealType });

        // MealPlan relationship with AppUser
        modelBuilder.Entity<MealPlan>()
            .HasOne<AppUser>()
            .WithMany(u => u.MealPlans)
            .HasForeignKey(mp => mp.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Index for hurtig opslag på UserId
        modelBuilder.Entity<MealPlan>()
            .HasIndex(mp => mp.UserId);

        // Group
        modelBuilder.Entity<Group>()
            .HasMany(g => g.Members)
            .WithOne()
            .HasForeignKey(gm => gm.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // GroupMember
        modelBuilder.Entity<GroupMember>()
            .HasIndex(gm => new { gm.GroupId, gm.UserId })
            .IsUnique();

        // GroupInvite
        modelBuilder.Entity<GroupInvite>()
            .HasOne<Group>()
            .WithMany()
            .HasForeignKey(gi => gi.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GroupInvite>()
            .HasIndex(gi => new { gi.GroupId, gi.UserId });

        // Match
        modelBuilder.Entity<Match>()
            .HasOne<Group>()
            .WithMany(g => g.Matches)
            .HasForeignKey(m => m.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Match>()
            .HasOne<Recipe>()
            .WithMany()
            .HasForeignKey(m => m.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Match>()
            .HasIndex(m => new { m.GroupId, m.RecipeId })
            .IsUnique();
    }
}