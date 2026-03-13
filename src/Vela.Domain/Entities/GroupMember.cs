namespace Vela.Domain.Entities;

public class GroupMember
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public required string UserId { get; set; }
    public string? Role { get; set; }
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
}