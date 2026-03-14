using Vela.Domain.Enums;

namespace Vela.Application.DTOs.Group;

public class GroupMemberDto
{
    public required Guid GroupId { get; set; }
    public required string UserId { get; set; }
    public GroupRole Role { get; set; }
    public DateTimeOffset JoinedAt { get; set; }
}