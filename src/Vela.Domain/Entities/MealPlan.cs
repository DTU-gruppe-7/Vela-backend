namespace Vela.Domain.Entities;

public class MealPlan
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public List<MealPlanEntry> Entries { get; set; } = new();
}
