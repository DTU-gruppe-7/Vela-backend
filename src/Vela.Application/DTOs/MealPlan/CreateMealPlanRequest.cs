namespace Vela.Application.DTOs.MealPlan;

// Request DTOs for controller
public class CreateMealPlanRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}