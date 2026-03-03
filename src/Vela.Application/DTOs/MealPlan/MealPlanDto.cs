namespace Vela.Application.DTOs.MealPlan;

public class MealPlanDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<MealPlanEntryDto> Entries { get; set; } = new();
}
