using Vela.Application.DTOs;

namespace Vela.Application.Interfaces.External.MealDb;

public interface IMealDbApiClient
{
    Task<MealDbResponse?> GetMealByLetterAsync(string letter);
}