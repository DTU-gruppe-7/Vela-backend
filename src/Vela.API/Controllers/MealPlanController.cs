using Microsoft.AspNetCore.Mvc;
using Vela.Application.DTOs.MealPlan;
using Vela.Application.Interfaces.Service;

namespace Vela.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MealPlanController(IMealPlanService mealPlanService) : BaseApiController
{
    private readonly IMealPlanService _mealPlanService = mealPlanService;

    [HttpGet]
    public async Task<IActionResult> GetAllMealPlans()
    {
        var result = await _mealPlanService.GetAllMealPlansAsync();
        return Ok(result.Data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMealPlan(Guid id)
    {
        var result = await _mealPlanService.GetMealPlanWithEntriesAsync(id);
        if (!result.Success)
            return NotFound(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMealPlan([FromBody] CreateMealPlanRequest request)
    {
        var result = await _mealPlanService.CreateMealPlanAsync(request.Name, request.Description);
        return CreatedAtAction(nameof(GetMealPlan), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMealPlan(Guid id, [FromBody] UpdateMealPlanRequest request)
    {
        var updateResult = await _mealPlanService.UpdateMealPlanAsync(id, request.Name, request.Description);
        if (!updateResult.Success)
            return NotFound(new { message = updateResult.ErrorMessage });

        var getResult = await _mealPlanService.GetMealPlanWithEntriesAsync(id);
        return Ok(getResult.Data);
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
}