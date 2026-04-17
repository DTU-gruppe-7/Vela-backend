namespace Vela.Application.DTOs.MealPlan;

public class UpdateMealPlanEntryServingsRequest
{
    public int Servings { get; set; }
    public DateOnly? Date { get; set; }
}
