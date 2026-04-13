namespace Vela.Application.DTOs.ShoppingList;

public class GenerateFromMealPlanRequestDto
{
    public List<Guid> ExcludedMealPlanEntryIds { get; set; } = new();
}