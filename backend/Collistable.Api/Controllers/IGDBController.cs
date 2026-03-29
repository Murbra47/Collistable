using Microsoft.AspNetCore.Mvc;
using Collistable.Api.Services;

namespace Collistable.Api.Controllers;

[ApiController]
[Route("api/igdb")]
public class IGDBController(IGDBService service) : ControllerBase
{
    private readonly IGDBService _service = service;

    // GET: api/igdb/search?q=game_name
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var json = await _service.SearchGamesAsync(q);
        return Content(json, "application/json");
    }
}
