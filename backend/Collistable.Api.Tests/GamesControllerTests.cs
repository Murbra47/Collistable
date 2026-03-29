using System.Security.Claims;
using Collistable.Api.Controllers;
using Collistable.Api.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Collistable.Api.Tests;

public class GamesControllerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static GamesController CreateController(AppDbContext db, int userId)
    {
        var controller = new GamesController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                }))
            }
        };
        return controller;
    }

    [Fact]
    public async Task GetGames_ReturnsOnlyCurrentUsersGames()
    {
        using var db = CreateDb();
        db.Games.AddRange(
            new Game { Title = "User1 Game", Platform = "PC", UserId = 1 },
            new Game { Title = "User2 Game", Platform = "PS5", UserId = 2 }
        );
        await db.SaveChangesAsync();

        var controller = CreateController(db, userId: 1);
        var result = await controller.GetGames();

        var games = Assert.IsAssignableFrom<IEnumerable<Game>>(result.Value);
        Assert.Single(games);
        Assert.Equal("User1 Game", games.First().Title);
    }

    [Fact]
    public async Task GetGame_ReturnsGame_WhenOwnedByCurrentUser()
    {
        using var db = CreateDb();
        db.Games.Add(new Game { Id = 1, Title = "My Game", Platform = "PC", UserId = 1 });
        await db.SaveChangesAsync();

        var controller = CreateController(db, userId: 1);
        var result = await controller.GetGame(1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var game = Assert.IsType<Game>(ok.Value);
        Assert.Equal("My Game", game.Title);
    }

    [Fact]
    public async Task GetGame_ReturnsNotFound_WhenGameBelongsToAnotherUser()
    {
        using var db = CreateDb();
        db.Games.Add(new Game { Id = 1, Title = "Other Game", Platform = "PC", UserId = 2 });
        await db.SaveChangesAsync();

        var controller = CreateController(db, userId: 1);
        var result = await controller.GetGame(1);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetGame_ReturnsNotFound_WhenGameDoesNotExist()
    {
        using var db = CreateDb();

        var controller = CreateController(db, userId: 1);
        var result = await controller.GetGame(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateGame_SetsUserIdFromToken()
    {
        using var db = CreateDb();
        var controller = CreateController(db, userId: 5);

        var game = new Game { Title = "New Game", Platform = "Xbox" };
        await controller.CreateGame(game);

        var saved = await db.Games.FirstAsync();
        Assert.Equal(5, saved.UserId);
    }

    [Fact]
    public async Task CreateGame_SetsTimestampsFromBackend()
    {
        using var db = CreateDb();
        var controller = CreateController(db, userId: 1);
        var before = DateTime.UtcNow;

        var game = new Game { Title = "New Game", Platform = "Xbox", CreatedAt = DateTime.MinValue, UpdatedAt = DateTime.MinValue };
        await controller.CreateGame(game);

        var saved = await db.Games.FirstAsync();
        Assert.True(saved.CreatedAt >= before);
        Assert.True(saved.UpdatedAt >= before);
    }

    [Fact]
    public async Task UpdateGame_ReturnsForbid_WhenGameBelongsToAnotherUser()
    {
        using var db = CreateDb();
        db.Games.Add(new Game { Id = 1, Title = "Other Game", Platform = "PC", UserId = 2 });
        await db.SaveChangesAsync();

        var controller = CreateController(db, userId: 1);
        var update = new Game { Id = 1, Title = "Hacked", Platform = "PC", UserId = 1 };
        var result = await controller.UpdateGame(1, update);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateGame_SetsUpdatedAtFromBackend()
    {
        using var db = CreateDb();
        db.Games.Add(new Game { Id = 1, Title = "My Game", Platform = "PC", UserId = 1, UpdatedAt = DateTime.MinValue });
        await db.SaveChangesAsync();

        var controller = CreateController(db, userId: 1);
        var before = DateTime.UtcNow;

        var update = new Game { Id = 1, Title = "Updated", Platform = "PC", UserId = 1, UpdatedAt = DateTime.MinValue };
        await controller.UpdateGame(1, update);

        var saved = await db.Games.FirstAsync();
        Assert.True(saved.UpdatedAt >= before);
    }

    [Fact]
    public async Task DeleteGame_ReturnsForbid_WhenGameBelongsToAnotherUser()
    {
        using var db = CreateDb();
        db.Games.Add(new Game { Id = 1, Title = "Other Game", Platform = "PC", UserId = 2 });
        await db.SaveChangesAsync();

        var controller = CreateController(db, userId: 1);
        var result = await controller.DeleteGame(1);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeleteGame_RemovesGame_WhenOwnedByCurrentUser()
    {
        using var db = CreateDb();
        db.Games.Add(new Game { Id = 1, Title = "My Game", Platform = "PC", UserId = 1 });
        await db.SaveChangesAsync();

        var controller = CreateController(db, userId: 1);
        var result = await controller.DeleteGame(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(db.Games);
    }

    [Fact]
    public async Task Import_ReturnsConflict_WhenIgdbGameAlreadyInUsersCollection()
    {
        using var db = CreateDb();
        db.Games.Add(new Game { Title = "Existing", Platform = "PC", IgdbId = 42, UserId = 1 });
        await db.SaveChangesAsync();

        var controller = CreateController(db, userId: 1);
        var result = await controller.Import(new Game { Title = "Existing", Platform = "PC", IgdbId = 42 });

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Import_Succeeds_WhenSameIgdbGameOwnedByDifferentUser()
    {
        using var db = CreateDb();
        db.Games.Add(new Game { Title = "Existing", Platform = "PC", IgdbId = 42, UserId = 2 });
        await db.SaveChangesAsync();

        var controller = CreateController(db, userId: 1);
        var result = await controller.Import(new Game { Title = "Existing", Platform = "PC", IgdbId = 42 });

        Assert.IsType<CreatedAtActionResult>(result);
    }
}
