using Microsoft.AspNetCore.Mvc;
using Vela.Application.Interfaces.External;

namespace Vela.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class adminController : ControllerBase
{
    private readonly IRecipeImportService _importService;

    public adminController(IRecipeImportService importService)
    {
        _importService = importService;
    }

    [HttpPost("import-all-meals")]
    public async Task<IActionResult> ImportAllMeals()
    {
        await _importService.ImportRecipesFromJsonAsync();
        
        return Ok(new { message = "Import succeeded! All recipes from the JSON-file has been imported." });
    }
}