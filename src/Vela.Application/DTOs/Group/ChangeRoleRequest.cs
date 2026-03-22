using Vela.Domain.Enums;

namespace Vela.Application.DTOs.Group;

public class ChangeRoleRequest
{
    public required GroupRole NewRole { get; set; }
}