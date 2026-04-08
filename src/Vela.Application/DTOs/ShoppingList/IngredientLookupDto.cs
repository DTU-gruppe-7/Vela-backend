using Vela.Domain.Enums;

namespace Vela.Application.DTOs.ShoppingList;

public class IngredientLookupDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Unit { get; set; }
    public IngredientCategory Category { get; set; }
}