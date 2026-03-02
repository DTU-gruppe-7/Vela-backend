using Vela.Application.DTOs.Recipe;

namespace Vela.Application.DTOs.MealPlan;

public class MealPlanEntryDto
{
    public Guid Id { get; set; }
    public Guid MealPlanId { get; set; }
    public Guid RecipeId { get; set; }
    public required string Day { get; set; }
    public required string MealType { get; set; }
    public int Servings { get; set; }
    public DateTime AddedAt { get; set; }
    public RecipeSummaryDto? Recipe { get; set; }
}
