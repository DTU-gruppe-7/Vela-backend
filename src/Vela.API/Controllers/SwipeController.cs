using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Vela.Application.DTOs;
using Vela.Application.Interfaces.Service;

namespace Vela.API.Controllers
{
    [Authorize]
    public class SwipeController(ILikeService likeService) : BaseApiController
    {
        private readonly ILikeService _likeService = likeService;

        [HttpPost]
        public async Task<IActionResult> RecordSwipe([FromBody] SwipeDto swipeDto)
        {
            var userId = GetCurrentUserId();
            var result = await _likeService.RecordSwipeAsync(userId, swipeDto);

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
            var likedRecipes = await _likeService.GetLikedRecipesByUserIdAsync(userId);
            return Ok(likedRecipes);
        }
    }
}
