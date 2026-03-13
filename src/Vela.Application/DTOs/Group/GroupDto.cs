using Vela.Application.DTOs.MealPlan;

namespace Vela.Application.DTOs.Group;

public class GroupDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public List<GroupMemberDto> Members { get; set; } = new();
    public MealPlanDto? MealPlan { get; set; }
}