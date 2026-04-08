using Vela.Domain.Enums;

namespace Vela.Domain.Entities.Recipe;

public class Ingredient
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string Unit { get; set; } = "Not set";
    public IngredientCategory Category { get; set; }
    public bool ContainsGluten { get; set; }
    public bool ContainsLactose { get; set; }
    public bool ContainsNuts { get; set; }
    public bool IsVegan { get; set; }
}