using Collistable.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Collistable.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GamesController(AppDbContext db) : ControllerBase
{

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User ID claim missing — endpoint must be protected by [Authorize]"));

    // GET: api/games
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Game>>> GetGames()
    {
        return await db.Games
            .Where(g => g.UserId == CurrentUserId)
            .ToListAsync();
    }

    // GET: api/games/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Game>> GetGame(int id)
    {
        // Single query with ownership filter — avoids leaking resource existence to non-owners
        var game = await db.Games.FirstOrDefaultAsync(g => g.Id == id && g.UserId == CurrentUserId);
        if (game is null) return NotFound();
        return Ok(game);
    }

    // POST: api/games
    [HttpPost]
    public async Task<ActionResult<Game>> CreateGame(Game game)
    {
        game.UserId = CurrentUserId;
        game.CreatedAt = DateTime.UtcNow;
        game.UpdatedAt = DateTime.UtcNow;
        db.Games.Add(game);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetGame), new { id = game.Id }, game);
    }

    // PUT: api/games/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGame(int id, Game updatedGame)
    {
        if (id != updatedGame.Id) return BadRequest();

        var game = await db.Games.FindAsync(id);
        if (game is null) return NotFound();
        if (game.UserId != CurrentUserId) return Forbid();

        updatedGame.UserId = CurrentUserId;
        updatedGame.UpdatedAt = DateTime.UtcNow;
        db.Entry(game).CurrentValues.SetValues(updatedGame);
        await db.SaveChangesAsync();

        return Ok(game);
    }

    // DELETE: api/games/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGame(int id)
    {
        var game = await db.Games.FindAsync(id);
        if (game is null) return NotFound();
        if (game.UserId != CurrentUserId) return Forbid();

        db.Games.Remove(game);
        await db.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/games/import
    [HttpPost("import")]
    public async Task<IActionResult> Import([FromBody] Game game)
    {
        if (game.IgdbId is null)
            return BadRequest("IGDB Id is required");

        // Deduplication is scoped per user — two users can both own the same IGDB game
        var existing = await db.Games
            .FirstOrDefaultAsync(g => g.IgdbId == game.IgdbId && g.UserId == CurrentUserId);

        if (existing != null)
            return Conflict("Game already exists in collection");

        game.UserId = CurrentUserId;
        game.CreatedAt = DateTime.UtcNow;
        game.UpdatedAt = DateTime.UtcNow;

        db.Games.Add(game);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGame), new { id = game.Id }, game);
    }
}
