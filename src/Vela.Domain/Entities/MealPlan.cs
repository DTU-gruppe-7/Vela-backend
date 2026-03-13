namespace Vela.Domain.Entities;

public class MealPlan
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public Guid? GroupId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation property
    public List<MealPlanEntry> Entries { get; set; } = new();
}
