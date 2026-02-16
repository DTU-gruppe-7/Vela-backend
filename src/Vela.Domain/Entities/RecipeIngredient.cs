namespace Vela.Domain.Entities;

public class RecipeIngredient
{
    public Guid RecipeId { get; set; }
    public Recipe Recipe { get; set; }
    
    public Guid IngredientId { get; set; }
    public Ingredient Ingredient { get; set; }
    
    public double Quantity { get; set; }
    public string Unit { get; set; }
    public string RawMeasure { get; set; }
}