namespace Vela.Application.DTOs;

public class AddShoppingListItemDto
{
    public required string IngredientName { get; set; }
    public double Quantity { get; set; }
    public string? Unit { get; set; }
    public string? AssignedUserId { get; set; }
}