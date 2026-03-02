using Microsoft.AspNetCore.Mvc;
using Vela.Application.DTOs.MealPlan;
using Vela.Application.Interfaces.Service;

namespace Vela.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MealPlanController : ControllerBase
{
    private readonly IMealPlanService _mealPlanService;

    public MealPlanController(IMealPlanService mealPlanService)
    {
        _mealPlanService = mealPlanService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllMealPlans()
    {
        var mealPlans = await _mealPlanService.GetAllMealPlansAsync();
        return Ok(mealPlans);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMealPlan(Guid id)
    {
        var mealPlan = await _mealPlanService.GetMealPlanWithEntriesAsync(id);
        if (mealPlan == null)
            return NotFound(new { message = $"Meal plan with ID {id} not found" });

        return Ok(mealPlan);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMealPlan([FromBody] CreateMealPlanRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Meal plan name is required" });

        var mealPlan = await _mealPlanService.CreateMealPlanAsync(request.Name, request.Description);
        return CreatedAtAction(nameof(GetMealPlan), new { id = mealPlan.Id }, mealPlan);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMealPlan(Guid id, [FromBody] UpdateMealPlanRequest request)
    {
        try
        {
            await _mealPlanService.UpdateMealPlanAsync(id, request.Name, request.Description);
            var updated = await _mealPlanService.GetMealPlanWithEntriesAsync(id);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Meal plan with ID {id} not found" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMealPlan(Guid id)
    {
        try
        {
            await _mealPlanService.DeleteMealPlanAsync(id);
            return Ok(new { message = "Meal plan deleted successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Meal plan with ID {id} not found" });
        }
    }

    [HttpPost("{mealPlanId}/entries")]
    public async Task<IActionResult> AddRecipeToMealPlan(Guid mealPlanId, [FromBody] AddMealPlanEntryRequest request)
    {
        try
        {
            var entry = await _mealPlanService.AddRecipeToMealPlanAsync(mealPlanId, request);
            return CreatedAtAction(nameof(GetMealPlan), new { id = mealPlanId }, entry);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{mealPlanId}/entries/{entryId}")]
    public async Task<IActionResult> RemoveRecipeFromMealPlan(Guid mealPlanId, Guid entryId)
    {
        try
        {
            await _mealPlanService.RemoveRecipeFromMealPlanAsync(mealPlanId, entryId);
            return Ok(new { message = "Recipe removed from meal plan successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

// Request DTOs for controller
public class CreateMealPlanRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}

public class UpdateMealPlanRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
