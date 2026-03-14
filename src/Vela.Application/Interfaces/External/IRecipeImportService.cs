namespace Vela.Application.Interfaces.External;

public interface IRecipeImportService
{
    Task ImportRecipesFromJsonAsync();
}