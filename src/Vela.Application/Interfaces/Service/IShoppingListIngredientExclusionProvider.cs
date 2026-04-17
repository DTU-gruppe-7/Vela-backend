namespace Vela.Application.Interfaces.Service;

public interface IShoppingListIngredientExclusionProvider
{
    bool IsExcluded(string ingredientName);
}

