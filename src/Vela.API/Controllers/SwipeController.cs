using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.Service;

namespace Vela.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SwipeController(ISwipeService swipeService) : BaseApiController
    {
        private readonly ISwipeService _swipeService = swipeService;

        [HttpPost]
        public async Task<IActionResult> RecordSwipe([FromBody] SwipeDto swipeDto)
        {
            var userId = GetCurrentUserId();
            var result = await _swipeService.RecordSwipeAsync(userId, swipeDto);

            if (!result.Success)
            {
                return NotFound(new { message = result.ErrorMessage });
            }

            return Ok();
        }

        [HttpGet("liked")]

        public async Task<IActionResult> GetLikedRecipesByUserIdAsync()
        {
            var userId = GetCurrentUserId();
            var likedRecipes = await _swipeService.GetLikedRecipesByUserIdAsync(userId);
            return Ok(likedRecipes);
        }
    }
}
