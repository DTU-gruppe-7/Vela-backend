using System.Text.Json.Serialization;

namespace Vela.Application.DTOs.Group;

public record UpdateGroupNameRequest(
    [property: JsonPropertyName("name")] string Name
);
