using System.Text.Json.Serialization;

namespace Vela.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationType
{
    GroupInvite,
    GroupDeclined,
    GroupAccepted,
    NewMatch
}