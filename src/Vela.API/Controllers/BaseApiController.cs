using Microsoft.AspNetCore.Mvc;
using System;

namespace Vela.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseApiController : ControllerBase
    {
        protected Guid GetCurrentUserId()
        {
            // In a real application, you would extract the user ID from the JWT token or session.
            // For example: User.FindFirstValue(ClaimTypes.NameIdentifier)
            // For this example, we'll just return a fixed GUID for testing purposes.
            return Guid.Parse("00000000-0000-0000-0000-000000000001");
        }
    }
}
