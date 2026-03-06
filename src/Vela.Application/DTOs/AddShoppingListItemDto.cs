namespace Vela.Application.DTOs;

public class AddShoppingListItemDto
{
    public Guid IngredientId { get; set; }
    public double Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal? Price { get; set; }
    public string? Shop { get; set; }
}