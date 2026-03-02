using Microsoft.AspNetCore.Mvc;
using Vela.Application.Interfaces.External;

namespace Vela.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController(IRecipeImportService importService) : BaseApiController
{
    private readonly IRecipeImportService _importService = importService;

    [HttpPost("import-all-meals")]
    public async Task<IActionResult> ImportAllMeals()
    {
        await _importService.ImportRecipesFromJsonAsync();
        
        return Ok(new { message = "Import succeeded! All recipes from the JSON-file has been imported." });
    }
}