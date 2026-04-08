using Vela.Domain.Enums;

namespace Vela.Application.DTOs.ShoppingList;

public class AddShoppingListItemDto
{
    public Guid? IngredientId { get; set; }
    public required string IngredientName { get; set; }
    public IngredientCategory Category { get; set; }
    public double Quantity { get; set; }
    public required string Unit { get; set; }
    public string? AssignedUserId { get; set; }
}