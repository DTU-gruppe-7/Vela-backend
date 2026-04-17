using Vela.Application.Interfaces.Service;

namespace Vela.API.Configuration;

public class ShoppingListIngredientExclusionProvider(IConfiguration configuration) : IShoppingListIngredientExclusionProvider
{
    private readonly HashSet<string> _excludedIngredientNames = configuration
        .GetSection("ShoppingList:ExcludedIngredientNames")
        .Get<string[]>()?
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Select(x => x.Trim().ToLowerInvariant())
        .ToHashSet(StringComparer.OrdinalIgnoreCase)
        ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public bool IsExcluded(string ingredientName)
    {
        if (string.IsNullOrWhiteSpace(ingredientName))
            return false;

        return _excludedIngredientNames.Contains(ingredientName.Trim().ToLowerInvariant());
    }
}

