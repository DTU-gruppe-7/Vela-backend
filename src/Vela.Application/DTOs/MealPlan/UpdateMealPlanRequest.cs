namespace Vela.Application.DTOs.MealPlan;

public class UpdateMealPlanRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}