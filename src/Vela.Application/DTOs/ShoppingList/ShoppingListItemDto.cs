using Vela.Domain.Enums;

namespace Vela.Application.DTOs.ShoppingList;

public class ShoppingListItemDto
{
    public Guid Id { get; set; }
    public required string IngredientName { get; set; }
    public string? AssignedUserId { get; set; }
    public string? RecipeName { get; set; }
    public IngredientCategory  Category { get; set; }
    public double Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal? Price { get; set; }
    public string? Shop { get; set; }
    public bool IsBought { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}