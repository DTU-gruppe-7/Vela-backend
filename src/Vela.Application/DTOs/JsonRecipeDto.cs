using System.Text.Json.Serialization;

namespace Vela.Application.DTOs;

public class JsonRecipeDto
{
    [JsonPropertyName("titel")]
    public string Titel { get; set; } = null!;

    [JsonPropertyName("beskrivelse")]
    public string Beskrivelse { get; set; } = null!;

    [JsonPropertyName("kategori")]
    public string? Kategori { get; set; }

    [JsonPropertyName("noegleord")]
    public List<string> Noegleord { get; set; } = new();

    [JsonPropertyName("antal_personer")]
    public string AntalPersoner { get; set; } = null!;

    [JsonPropertyName("ingredienser")]
    public Dictionary<string, List<string>> Ingredienser { get; set; } = new();

    [JsonPropertyName("instruktioner")]
    public Dictionary<string, List<string>> Instruktioner { get; set; } = new();

    [JsonPropertyName("tid_i_alt")]
    public string? TidIAlt { get; set; }

    [JsonPropertyName("arbejdstid")]
    public string? Arbejdstid { get; set; }

    [JsonPropertyName("billede")]
    public string? Billede { get; set; }

    [JsonPropertyName("kilde_url")]
    public string? KildeUrl { get; set; }
}