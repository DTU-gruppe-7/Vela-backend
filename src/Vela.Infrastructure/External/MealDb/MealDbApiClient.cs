using System.Net.Http.Json;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.External.MealDb;

namespace Vela.Infrastructure.External.MealDb;

public class MealDbApiClient : IMealDbApiClient
{
    private readonly HttpClient _client;

    public MealDbApiClient(HttpClient client)
    {
        _client = client;
        _client.BaseAddress = new Uri("https://www.themealdb.com/api/json/v1/1/");
    }

    public async Task<MealDbResponse?> GetMealByLetterAsync(string letter)
    {
        return await _client.GetFromJsonAsync<MealDbResponse?>($"search.php?f={letter}");
    }
}