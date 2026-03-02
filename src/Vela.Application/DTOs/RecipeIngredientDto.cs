namespace Vela.Application.DTOs;

public class RecipeIngredientDto
{
    public Guid Id { get; set; }
    public Guid IngredientId { get; set; }
    public required string IngredientName { get; set; }
    public double Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Section { get; set; }
}