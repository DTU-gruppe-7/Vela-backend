using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Vela.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected string GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            
            return userId;
        }
        
        protected string? GetCurrentUserEmail()
        {
            return User.FindFirstValue(ClaimTypes.Email);
        }
    }
}