using Vela.Domain.Entities.Recipes;

namespace Vela.Application.Interfaces.Validation;

public interface IIngredientValidator
{
/// <summary>
/// Validates an ingredient entity for all business rules and data consistency.
/// </summary>
/// <returns>ValidationResult with success status and any error/warning messages</returns>
ValidationResult ValidateIngredient(Ingredient ingredient);

/// <summary>
/// Validates ingredient name format and content.
/// </summary>
ValidationResult ValidateName(string name);

/// <summary>
/// Validates logical consistency between category, allergens, and vegan status.
/// </summary>
ValidationResult ValidateConsistency(Ingredient ingredient);

/// <summary>
/// Validates that the unit is appropriate for the ingredient category.
/// </summary>
ValidationResult ValidateUnit(string unit, Vela.Domain.Enums.IngredientCategory category);
}

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public class ValidationResult
{
public bool IsValid { get; set; }
public List<string> Errors { get; set; } = new();
public List<string> Warnings { get; set; } = new();

public static ValidationResult Success() => new() { IsValid = true };

public static ValidationResult Failure(params string[] errors)
=> new() { IsValid = false, Errors = errors.ToList() };

public void AddError(string error)
{
IsValid = false;
Errors.Add(error);
}

public void AddWarning(string warning)
{
Warnings.Add(warning);
}
}
