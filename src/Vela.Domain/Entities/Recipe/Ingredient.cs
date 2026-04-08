using System.ComponentModel.DataAnnotations;
using Vela.Domain.Enums;

namespace Vela.Domain.Entities.Recipe;

public class Ingredient
{
	public Guid Id { get; set; }

	[Required]
	[MinLength(1, ErrorMessage = "Ingredient name cannot be empty")]
	[MaxLength(200, ErrorMessage = "Ingredient name cannot exceed 200 characters")]
	public required string Name { get; set; }

	[Required]
	[AllowedValues("g", "ml", "stk", "Not set", ErrorMessage = "Unit must be one of: g, ml, stk, Not set")]
	public string Unit { get; set; } = "Not set";

	public IngredientCategory Category { get; set; }
	public bool ContainsGluten { get; set; }
	public bool ContainsLactose { get; set; }
	public bool ContainsNuts { get; set; }
	public bool IsVegan { get; set; }
}