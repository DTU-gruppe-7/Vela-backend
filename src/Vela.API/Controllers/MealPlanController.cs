using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vela.Application.DTOs.MealPlan;
using Vela.Application.Interfaces.Service;

namespace Vela.API.Controllers;

[Authorize]
public class MealPlanController(IMealPlanService mealPlanService) : BaseApiController
{
    private readonly IMealPlanService _mealPlanService = mealPlanService;

    [HttpGet]
    public async Task<ActionResult<MealPlanDto>> GetMealPlan(
        [FromQuery] Guid groupId,
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate)
    {
        if (groupId.Equals(Guid.Empty))
        {
            var currentUserId = GetCurrentUserId();
            var result = await _mealPlanService.GetAggregatedMealPlanAsync(currentUserId, startDate, endDate);
            if (!result.Success)
                return NotFound(new { message = result.ErrorMessage });
            return Ok(result.Data);
        }
        else
        {
            var result = await _mealPlanService.GetMealPlanAsync(null, groupId, startDate, endDate);
            if (!result.Success)
                return NotFound(new { message = result.ErrorMessage });
            return Ok(result.Data);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMealPlan(Guid id, [FromBody] UpdateMealPlanRequest request)
    {
        var updateResult = await _mealPlanService.UpdateMealPlanAsync(id, request.Name, request.Description);
        if (!updateResult.Success)
            return NotFound(new { message = updateResult.ErrorMessage });
        
        return Ok(updateResult);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMealPlan(Guid id)
    {
        var result = await _mealPlanService.DeleteMealPlanAsync(id);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(new { message = "Meal plan deleted successfully" });
    }

    [HttpPost("{mealPlanId}/entries")]
    public async Task<IActionResult> AddRecipeToMealPlan(Guid mealPlanId, [FromBody] AddMealPlanEntryRequest request)
    {
        var result = await _mealPlanService.AddRecipeToMealPlanAsync(mealPlanId, request);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        return CreatedAtAction(nameof(GetMealPlan), new { id = mealPlanId }, result.Data);
    }

    [HttpDelete("{mealPlanId}/entries/{entryId}")]
    public async Task<IActionResult> RemoveRecipeFromMealPlan(Guid mealPlanId, Guid entryId)
    {
        var result = await _mealPlanService.RemoveRecipeFromMealPlanAsync(mealPlanId, entryId);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Recipe removed from meal plan successfully" });
    }
    
    [HttpPut("{mealPlanId}/entries/{entryId}")]
    public async Task<IActionResult> UpdateMealPlanEntryServings(Guid mealPlanId, Guid entryId, [FromBody] UpdateMealPlanEntryServingsRequest request)
    {
        var result = await _mealPlanService.UpdateMealPlanEntryServingsAsync(mealPlanId, entryId, request.Servings);
        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Meal plan entry servings updated successfully" });
    }
}