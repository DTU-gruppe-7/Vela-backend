namespace Vela.Application.DTOs.Group;

public class GroupMemberDto
{
    public required Guid GroupId { get; set; }
    public required string UserId { get; set; }
    public string? Role { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
}