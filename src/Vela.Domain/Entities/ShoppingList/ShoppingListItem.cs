using Vela.Domain.Entities.MealPlan;
using Vela.Domain.Enums;

namespace Vela.Domain.Entities.ShoppingList;

public class ShoppingListItem
{
    public Guid Id  { get; set; }
    public Guid ShoppingListId { get; set; }
    public required string IngredientName { get; set; }
    public Guid? IngredientId { get; set; }
    public IngredientCategory ItemCategory { get; set; }
    public Guid? MealPlanEntryId { get; set; }
    public MealPlanEntry? MealPlanEntry { get; set; }
    public string? AssignedUserId { get; set; }
    public double Quantity { get; set; }
    public required string? Unit { get; set; }
    public decimal? Price { get; set; }
    public string? Shop { get; set; }
    public bool IsBought { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    
}