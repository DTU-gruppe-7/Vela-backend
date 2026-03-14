using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vela.Application.DTOs.Group;
using Vela.Application.Interfaces.Service;
using Vela.Infrastructure.Identity;

namespace Vela.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GroupController(
    IGroupService groupService, 
    IGroupInviteService groupInviteService, 
    IShoppingListService shoppingListService, 
    IMealPlanService mealPlanService,
    UserManager<AppUser> userManager) : BaseApiController
{
    private readonly IGroupService _groupService = groupService;
    private readonly IGroupInviteService _groupInviteService = groupInviteService;
    private readonly IMealPlanService _mealPlanService = mealPlanService;
    private readonly IShoppingListService _shoppingListService = shoppingListService;
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
        var result = await _groupService.GetGroupAsync(id);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<ActionResult<GroupDto>> CreateGroup([FromBody] CreateGroupRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _groupService.CreateGroupAsync(userId, request);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });
        
        var groupId = result.Data.Id;
        var groupName = result.Data.Name;
        
        var shoppingListResult = await _shoppingListService.CreateShoppingListAsync(null, groupId, groupName + "'s indkøbsliste");
        if (!shoppingListResult.Success){
            await _groupService.DeleteGroupAsync(groupId);
            return BadRequest(new { message = shoppingListResult.ErrorMessage });
        }
        
        var mealPlanResult = await _mealPlanService.CreateMealPlanAsync(null, groupId, groupName + "'s madplan");
        if (!mealPlanResult.Success)
        {
            await _groupService.DeleteGroupAsync(groupId);
            return BadRequest(new { message = mealPlanResult.ErrorMessage });
        }
            
        
        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGroup(Guid id)
    {
        var result = await _groupService.DeleteGroupAsync(id);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(new { message = "Group deleted successfully" });
    }

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberRequest request)
    {
        var result = await _groupService.AddMemberAsync(id, request);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Member added successfully" });
    }

    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(Guid id, string userId)
    {
        var result = await _groupService.RemoveMemberAsync(id, userId);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(new { message = "Member removed successfully" });
    }

    [HttpGet("{id}/matches")]
    public async Task<IActionResult> GetMatches(Guid id)
    {
        var result = await _groupService.GetMatchesAsync(id);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    // Invites
    [HttpPost("{id}/invites")]
    public async Task<IActionResult> SendInvite(Guid id, [FromBody] SendInviteRequest request)
    {
        var user = await  _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return NotFound(new { message = "Der findes ingen bruger med denne e-mail adresse." });
        }
        
        var existingInvites = await _groupInviteService.GetInvitesByGroupIdAsync(id);
        if (existingInvites.Data != null && existingInvites.Data.Any(i => i.UserId == user.Id))
        {
            return BadRequest(new { message = "Der er allerede sendt en invitation til denne bruger for denne gruppe." });
        }
        var result = await _groupInviteService.SendInviteAsync(user.Id, id);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Invite sent successfully" });
    }

    [HttpGet("{id}/invites")]
    public async Task<IActionResult> GetInvitesByGroup(Guid id)
    {
        var result = await _groupInviteService.GetInvitesByGroupIdAsync(id);
        return Ok(result.Data);
    }

    [HttpPatch("invites/{groupId}/accept")]
    public async Task<IActionResult> AcceptInvite(Guid groupId)
    {
        var userId = GetCurrentUserId();
        var result = await _groupInviteService.AcceptInviteAsync(userId, groupId);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(new { message = "Invite accepted" });
    }

    [HttpPatch("invites/{groupId}/decline")]
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
}