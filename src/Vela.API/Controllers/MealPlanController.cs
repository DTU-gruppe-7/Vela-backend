using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vela.Application.Common;
using Vela.Application.DTOs.MealPlan;
using Vela.Application.Interfaces.Service;

namespace Vela.API.Controllers;

[Authorize]
[ApiVersion("1.0")]
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
            var callerUserId = GetCurrentUserId();
            var result = await _mealPlanService.GetMealPlanAsync(null, groupId, startDate, endDate, callerUserId);
            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    ResultErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
                    ResultErrorType.Forbidden => StatusCode(403, new { message = result.ErrorMessage }),
                    _ => BadRequest(new { message = result.ErrorMessage })
                };
            }
            return Ok(result.Data);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMealPlan(Guid id, [FromBody] UpdateMealPlanRequest request)
    {
        var callerUserId = GetCurrentUserId();
        var updateResult = await _mealPlanService.UpdateMealPlanAsync(id, request.Name, request.Description, callerUserId);
        if (!updateResult.Success)
        {
            return updateResult.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(new { message = updateResult.ErrorMessage }),
                ResultErrorType.Forbidden => StatusCode(403, new { message = updateResult.ErrorMessage }),
                _ => BadRequest(new { message = updateResult.ErrorMessage })
            };
        }
        return Ok(updateResult);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMealPlan(Guid id)
    {
        var callerUserId = GetCurrentUserId();
        var result = await _mealPlanService.DeleteMealPlanAsync(id, callerUserId);
        if (!result.Success)
        {
            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ResultErrorType.Forbidden => StatusCode(403, new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }
        return Ok(new { message = "Meal plan deleted successfully" });
    }

    [HttpPost("{mealPlanId}/entries")]
    public async Task<IActionResult> AddRecipeToMealPlan(Guid mealPlanId, [FromBody] AddMealPlanEntryRequest request)
    {
        var callerUserId = GetCurrentUserId();
        var result = await _mealPlanService.AddRecipeToMealPlanAsync(mealPlanId, request, callerUserId);
        if (!result.Success)
        {
            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ResultErrorType.Forbidden => StatusCode(403, new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }
        return CreatedAtAction(nameof(GetMealPlan), new { id = mealPlanId }, result.Data);
    }

    [HttpDelete("{mealPlanId}/entries/{entryId}")]
    public async Task<IActionResult> RemoveRecipeFromMealPlan(Guid mealPlanId, Guid entryId)
    {
        var callerUserId = GetCurrentUserId();
        var result = await _mealPlanService.RemoveRecipeFromMealPlanAsync(mealPlanId, entryId, callerUserId);
        if (!result.Success)
        {
            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ResultErrorType.Forbidden => StatusCode(403, new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }
        return Ok(new { message = "Recipe removed from meal plan successfully" });
    }

    [HttpPut("{mealPlanId}/entries/{entryId}")]
    public async Task<IActionResult> UpdateMealPlanEntryServings(Guid mealPlanId, Guid entryId, [FromBody] UpdateMealPlanEntryServingsRequest request)
    {
        var callerUserId = GetCurrentUserId();
        var result = await _mealPlanService.UpdateMealPlanEntryServingsAsync(mealPlanId, entryId, request.Servings, request.Date, callerUserId);
        if (!result.Success)
        {
            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(new { message = result.ErrorMessage }),
                ResultErrorType.Forbidden => StatusCode(403, new { message = result.ErrorMessage }),
                _ => BadRequest(new { message = result.ErrorMessage })
            };
        }
        return Ok(new { message = "Meal plan entry servings updated successfully" });
    }
}