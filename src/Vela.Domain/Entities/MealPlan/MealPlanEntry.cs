using Vela.Domain.Entities.Recipes;

namespace Vela.Domain.Entities.MealPlan;

public class MealPlanEntry
{
    public Guid Id { get; set; }
    
    public Guid MealPlanId { get; set; }
    public required MealPlan MealPlan { get; set; }
    
    public Guid RecipeId { get; set; }
    public required Recipe Recipe { get; set; }
    
    public required DateOnly Date { get; set; } 
    public required string MealType { get; set; }
    public int Servings { get; set; } = 4;
    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool AddedToShoppingList { get; set; } = false;
}
