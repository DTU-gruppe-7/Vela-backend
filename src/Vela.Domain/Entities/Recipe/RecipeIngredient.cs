namespace Vela.Domain.Entities;

public class RecipeIngredient
{
    public Guid Id { get; set; }
    public Guid RecipeId { get; set; }
    public required Recipe Recipe { get; set; }

    public Guid IngredientId { get; set; }
    public required Ingredient Ingredient { get; set; }

    public double Quantity { get; set; }
    public string? Unit { get; set; }
    //Which section the ingredient is for
    public string? Section { get; set; }
    public required string RawMeasure { get; set; }
}