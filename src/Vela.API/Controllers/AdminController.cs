using Microsoft.AspNetCore.Mvc;
using Vela.Application.Interfaces.External.MealDb;

namespace Vela.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IMealDbImportService _importService;

    public AdminController(IMealDbImportService importService)
    {
        _importService = importService;
    }

    [HttpPost("import-all-meals")]
    public async Task<IActionResult> ImportAllMeals()
    {
        await _importService.ImportAllRecipesAsync();
        
        return Ok(new { message = "Import succeeded! All recipes A-Z have been imported." });
    }
}