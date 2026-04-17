using System.Text.RegularExpressions;
using Vela.Application.Interfaces.Validation;
using Vela.Domain.Entities.Recipes;
using Vela.Domain.Enums;

namespace Vela.Application.Validation;

public class IngredientValidator : IIngredientValidator
{
private static readonly string[] ValidUnits = { "g", "ml", "stk", "Not set" };
private static readonly Regex MeasurementPattern = new(@"\d+\s*%|\d+\s*(gram|kg|liter|ml|dl|cl)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
private static readonly Regex InstructionPattern = new(@"\(.*?\)|—|–|-\s+[a-z]", RegexOptions.Compiled);

public ValidationResult ValidateIngredient(Ingredient ingredient)
{
var result = new ValidationResult { IsValid = true };

// Validate name
var nameResult = ValidateName(ingredient.Name);
if (!nameResult.IsValid)
{
result.IsValid = false;
result.Errors.AddRange(nameResult.Errors);
}
result.Warnings.AddRange(nameResult.Warnings);

// Validate unit
var unitResult = ValidateUnit(ingredient.Unit, ingredient.Category);
if (!unitResult.IsValid)
{
result.IsValid = false;
result.Errors.AddRange(unitResult.Errors);
}
result.Warnings.AddRange(unitResult.Warnings);

// Validate consistency
var consistencyResult = ValidateConsistency(ingredient);
if (!consistencyResult.IsValid)
{
result.IsValid = false;
result.Errors.AddRange(consistencyResult.Errors);
}
result.Warnings.AddRange(consistencyResult.Warnings);

return result;
}

public ValidationResult ValidateName(string name)
{
var result = new ValidationResult { IsValid = true };

if (string.IsNullOrWhiteSpace(name))
{
result.AddError("Ingredient name cannot be empty");
return result;
}

if (name.Length > 200)
{
result.AddError($"Ingredient name exceeds maximum length of 200 characters (current: {name.Length})");
}

if (name != name.Trim())
{
result.AddWarning("Ingredient name has leading or trailing whitespace");
}

// Check for measurements in name
if (MeasurementPattern.IsMatch(name))
{
result.AddWarning($"Ingredient name contains measurement units: '{name}'. Consider moving to attributes.");
}

// Check for cooking instructions/descriptions
if (InstructionPattern.IsMatch(name) && name.Length > 50)
{
result.AddWarning($"Ingredient name appears to contain instructions or descriptions: '{name}'. Consider simplifying.");
}

// Check for unusual characters
if (name.Any(c => char.IsControl(c) && c != '\n' && c != '\r' && c != '\t'))
{
result.AddError("Ingredient name contains invalid control characters");
}

return result;
}

public ValidationResult ValidateConsistency(Ingredient ingredient)
{
var result = new ValidationResult { IsValid = true };

// Dairy products cannot be vegan
if (ingredient.Category == IngredientCategory.Dairy && ingredient.IsVegan)
{
result.AddError($"Ingredient '{ingredient.Name}' is categorized as Dairy but marked as vegan - logical contradiction");
}

// Lactose-containing items cannot be vegan
if (ingredient.ContainsLactose && ingredient.IsVegan)
{
result.AddError($"Ingredient '{ingredient.Name}' contains lactose but marked as vegan - logical contradiction");
}

// Meat, Fish, and Eggs cannot be vegan
if (ingredient.Category is IngredientCategory.Meat or IngredientCategory.Fish or IngredientCategory.Eggs
&& ingredient.IsVegan)
{
result.AddError($"Ingredient '{ingredient.Name}' is categorized as {ingredient.Category} but marked as vegan - logical contradiction");
}

// Warn about "Other" category
if (ingredient.Category == IngredientCategory.Other)
{
result.AddWarning($"Ingredient '{ingredient.Name}' is categorized as 'Other' - may need more specific classification");
}

return result;
}

public ValidationResult ValidateUnit(string unit, IngredientCategory category)
{
var result = new ValidationResult { IsValid = true };

if (string.IsNullOrWhiteSpace(unit))
{
result.AddError("Unit cannot be empty");
return result;
}

if (!ValidUnits.Contains(unit))
{
result.AddError($"Invalid unit '{unit}'. Must be one of: {string.Join(", ", ValidUnits)}");
}

// Category-specific unit recommendations (informational only)
switch (category)
{
case IngredientCategory.Beverages when unit != "ml":
result.AddWarning($"Beverages typically use 'ml' unit, but '{unit}' specified");
break;
case IngredientCategory.Meat when unit == "ml":
result.AddWarning($"Meat typically uses 'g' or 'stk' unit, but 'ml' specified");
break;
case IngredientCategory.Fish when unit == "ml":
result.AddWarning($"Fish typically uses 'g' unit, but 'ml' specified");
break;
}

return result;
}
}
