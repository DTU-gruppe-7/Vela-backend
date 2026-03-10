namespace Vela.Domain.Entities;

public class MealPlanEntry
{
    public Guid Id { get; set; }
    
    public Guid MealPlanId { get; set; }
    public required MealPlan MealPlan { get; set; }
    
    public Guid RecipeId { get; set; }
    public required Recipe Recipe { get; set; }
    
    public required DateOnly Date { get; set; } 
    public required string MealType { get; set; }
    public int Servings { get; set; } = 1;
    public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
}
