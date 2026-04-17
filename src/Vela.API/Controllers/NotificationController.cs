using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vela.Application.Interfaces.Service;
using Vela.Application.Interfaces.Service.Notification;

namespace Vela.API.Controllers;

[Authorize]
public class NotificationsController(INotificationService notificationService) : BaseApiController
{
    private readonly INotificationService _notificationService = notificationService;
    
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var userId = GetCurrentUserId();
        
        var result = await _notificationService.GetNotificationsAsync(userId);
        
        if (!result.Success) return BadRequest(result.ErrorMessage);
        
        return Ok(result);
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = GetCurrentUserId();

        var result = await _notificationService.MarkAsReadAsync(id, userId);

        if (!result.Success)
        { 
            var error = result.ErrorMessage ?? "Unknown error";
            if (error.Contains("Notification not found", StringComparison.Ordinal)) 
                return NotFound(error);
                
            if (error.Contains("Unauthorized", StringComparison.Ordinal)) 
                return Unauthorized(error);
            
            return BadRequest(result.ErrorMessage);
        }
        
        return Ok(result);
    }
}