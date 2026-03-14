namespace Vela.Application.DTOs.MealPlan;

public class AddMealPlanEntryRequest
{
    public required Guid RecipeId { get; set; }
    public required DateOnly Date { get; set; }
    public required string MealType { get; set; }
    public int Servings { get; set; } = 1;
}
