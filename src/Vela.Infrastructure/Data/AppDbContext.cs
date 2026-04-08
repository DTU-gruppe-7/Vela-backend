using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Vela.Domain.Entities;
using Vela.Domain.Entities.Group;
using Vela.Domain.Entities.MealPlan;
using Vela.Domain.Entities.Notification;
using Vela.Domain.Entities.Recipe;
using Vela.Infrastructure.Identity;
using Vela.Domain.Entities.ShoppingList;

namespace Vela.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<ShoppingList> ShoppingLists => Set<ShoppingList>();
    public DbSet<ShoppingListItem> ShoppingListItems => Set<ShoppingListItem>();
    public DbSet<MealPlan> MealPlans => Set<MealPlan>();
    public DbSet<MealPlanEntry> MealPlanEntries => Set<MealPlanEntry>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupMember> GroupMembers => Set<GroupMember>();
    public DbSet<GroupInvite> GroupInvites => Set<GroupInvite>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // RecipeIngredient bruger sit eget Id som PK
        modelBuilder.Entity<RecipeIngredient>()
            .HasKey(ri => ri.Id);

        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(ri => ri.Recipe)
            .WithMany(r => r.Ingredients)
            .HasForeignKey(ri => ri.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(ri => ri.Ingredient)
            .WithMany()
            .HasForeignKey(ri => ri.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index for hurtig opslag på Recipe + Ingredient
        modelBuilder.Entity<RecipeIngredient>()
            .HasIndex(ri => new { ri.RecipeId, ri.IngredientId });

        // Ingredient indexes for filtering and uniqueness
        modelBuilder.Entity<Ingredient>()
            .HasIndex(i => i.Name)
            .IsUnique();

        modelBuilder.Entity<Ingredient>()
            .HasIndex(i => i.Category);

        modelBuilder.Entity<Ingredient>()
            .HasIndex(i => i.IsVegan);

        modelBuilder.Entity<Ingredient>()
            .HasIndex(i => i.ContainsGluten);

        modelBuilder.Entity<Ingredient>()
            .HasIndex(i => i.ContainsLactose);

        modelBuilder.Entity<Ingredient>()
            .HasIndex(i => i.ContainsNuts);
        
        modelBuilder.Entity<Like>()
            .HasKey(s => s.LikeId);

        modelBuilder.Entity<Like>()
            .HasIndex(s => new { s.UserId,  s.RecipeId })
            .IsUnique();

        modelBuilder.Entity<Like>()
            .HasOne(l => l.Recipe)
            .WithMany()
            .HasForeignKey(l => l.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // ShoppingList -> AppUser (optional relationship)
        modelBuilder.Entity<ShoppingList>()
            .HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(sl => sl.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // ShoppingList -> Group (optional relationship)
        modelBuilder.Entity<ShoppingList>()
            .HasOne<Group>()
            .WithMany()
            .HasForeignKey(sl => sl.GroupId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Index for hurtig opslag på UserId
        modelBuilder.Entity<ShoppingList>()
            .HasIndex(sl => sl.UserId);
        
        // Index for hurtig opslag på GroupId
        modelBuilder.Entity<ShoppingList>()
            .HasIndex(sl => sl.GroupId);

        modelBuilder.Entity<ShoppingListItem>()
            .HasOne<ShoppingList>()
            .WithMany(sl => sl.Items)
            .HasForeignKey(sl => sl.ShoppingListId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<ShoppingListItem>()
            .HasOne<MealPlanEntry>()
            .WithMany()
            .HasForeignKey(i => i.MealPlanEntryId)
            .OnDelete(DeleteBehavior.Cascade);

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

        // MealPlan -> AppUser (optional relationship)
        modelBuilder.Entity<MealPlan>()
            .HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(mp => mp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // MealPlan -> Group (optional relationship)
        modelBuilder.Entity<MealPlan>()
            .HasOne<Group>()
            .WithMany()
            .HasForeignKey(mp => mp.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for hurtig opslag på UserId
        modelBuilder.Entity<MealPlan>()
            .HasIndex(mp => mp.UserId);
        
        // Index for hurtig opslag på GroupId
        modelBuilder.Entity<MealPlan>()
            .HasIndex(mp => mp.GroupId);

        // Group
        modelBuilder.Entity<Group>()
            .HasMany(g => g.Members)
            .WithOne()
            .HasForeignKey(gm => gm.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        // GroupMember
        modelBuilder.Entity<GroupMember>()
            .HasKey(gm => new { gm.GroupId, gm.UserId });

        modelBuilder.Entity<GroupMember>()
            .HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(gm => gm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // GroupInvite
        modelBuilder.Entity<GroupInvite>()
            .HasOne<Group>()
            .WithMany()
            .HasForeignKey(gi => gi.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GroupInvite>()
            .HasKey(gi => new { gi.GroupId, gi.UserId });
        
        //GroupRole enum
        modelBuilder.Entity<GroupMember>()
            .Property(gm => gm.Role)
            .HasConversion<string>();

        // Match
        modelBuilder.Entity<Match>()
            .HasKey(m => new { m.GroupId, m.RecipeId });

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

        // Notification
        modelBuilder.Entity<Notification>()
            .HasKey(n => n.Id);

        modelBuilder.Entity<Notification>()
            .HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasIndex(n => n.UserId);
    }
}