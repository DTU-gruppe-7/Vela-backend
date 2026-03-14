using Vela.Domain.Enums;

namespace Vela.Domain.Entities;

public class GroupMember
{
    public required Guid GroupId { get; set; }
    public required string UserId { get; set; }
    public required GroupRole Role { get; set; }
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
}