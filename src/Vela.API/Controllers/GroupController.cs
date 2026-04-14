using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vela.Application.DTOs.Group;
using Vela.Application.Interfaces.Service;
using Vela.Infrastructure.Identity;

namespace Vela.API.Controllers;

[Authorize]
public class GroupController(
    IGroupService groupService,
    IGroupInviteService groupInviteService,
    UserManager<AppUser> userManager) : BaseApiController
{
    private readonly IGroupService _groupService = groupService;
    private readonly IGroupInviteService _groupInviteService = groupInviteService;
    private readonly UserManager<AppUser> _userManager = userManager;
    

    [HttpGet]
    public async Task<ActionResult<GroupDto>> GetGroups()
    {
        var userId = GetCurrentUserId();
        var result = await _groupService.GetGroupsByUserIdAsync(userId);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetGroup(Guid id)
    {
        var callerUserId = GetCurrentUserId();
        var result = await _groupService.GetGroupAsync(id, callerUserId);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult<GroupDto>> CreateGroup([FromBody] CreateGroupRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _groupService.CreateGroupWithResourcesAsync(userId, request);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGroup(Guid id)
    {
        var callerUserId = GetCurrentUserId();
        var result = await _groupService.DeleteGroupAsync(id, callerUserId);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Group deleted successfully" });
    }

    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(Guid id, string userId)
    {
        var callerUserId = GetCurrentUserId();
        var result = await _groupService.RemoveMemberAsync(id, userId, callerUserId);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Member removed successfully" });
    }

    [HttpGet("{id}/matches")]
    public async Task<IActionResult> GetMatches(Guid id)
    {
        var callerUserId = GetCurrentUserId();
        var result = await _groupService.GetMatchesAsync(id, callerUserId);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpPatch("{id}/members/{userId}/role")]
    public async Task<IActionResult> ChangeMemberRole(Guid id, string userId, [FromBody] ChangeRoleRequest request)
    {
        var callerUserId = GetCurrentUserId();
        var result = await _groupService.ChangeMemberRoleAsync(id, userId, request.NewRole, callerUserId);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Role updated successfully" });
    }

    [HttpPost("{id}/leave")]
    public async Task<IActionResult> LeaveGroup(Guid id)
    {
        var callerUserId = GetCurrentUserId();
        var result = await _groupService.LeaveGroupAsync(id, callerUserId);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "You have left the group" });
    }

    [HttpPatch("{id}/transfer-ownership")]
    public async Task<IActionResult> TransferOwnership(Guid id, [FromBody] TransferOwnershipRequest request)
    {
        var callerUserId = GetCurrentUserId();
        var result = await _groupService.TransferOwnershipAsync(id, request.NewOwnerUserId, callerUserId);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Ownership transferred successfully" });
    }

    // Invites
    [HttpPost("{id}/invites")]
    public async Task<IActionResult> SendInvite(Guid id, [FromBody] SendInviteRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return NotFound(new { message = "Der findes ingen bruger med denne e-mail adresse." });

        var callerUserId = GetCurrentUserId();
        var result = await _groupInviteService.SendInviteAsync(user.Id, id, callerUserId);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Invite sent successfully" });
    }

    [HttpGet("{id}/invites")]
    public async Task<IActionResult> GetInvitesByGroup(Guid id)
    {
        var callerUserId = GetCurrentUserId();
        var result = await _groupInviteService.GetInvitesByGroupIdAsync(id, callerUserId);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpPatch("{groupId}/invites/accept")]
    public async Task<IActionResult> AcceptInvite(Guid groupId)
    {
        var userId = GetCurrentUserId();
        var result = await _groupInviteService.AcceptInviteAsync(userId, groupId);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(new { message = "Invite accepted" });
    }

    [HttpPatch("{groupId}/invites/decline")]
    public async Task<IActionResult> DeclineInvite(Guid groupId)
    {
        var userId = GetCurrentUserId();
        var result = await _groupInviteService.DeclineInviteAsync(userId, groupId);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(new { message = "Invite declined" });
    }

    [HttpGet("invites")]
    public async Task<IActionResult> GetMyInvites()
    {
        var userId = GetCurrentUserId();
        var result = await _groupInviteService.GetInvitesByUserIdAsync(userId);
        return Ok(result.Data);
    }
    
    [HttpPatch("{groupId}")]
    public async Task<IActionResult> UpdateName(Guid groupId, [FromBody] UpdateGroupNameRequest request)
    {
        var callerUserId = GetCurrentUserId(); 
        var result = await _groupService.UpdateGroupNameAsync(groupId, request.Name, callerUserId);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });
        
        return Ok(new { message = "Navnet er opdateret" });
    }
}