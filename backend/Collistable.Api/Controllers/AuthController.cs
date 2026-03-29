using Collistable.Api.Data;
using Collistable.Api.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Collistable.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, TokenService tokenService, IConfiguration config)
    {
        _db = db;
        _tokenService = tokenService;
        _config = config;
    }

    // POST: api/auth/google
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _config["Google:ClientId"] }
        };

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential, settings);
        }
        catch (InvalidJwtException)
        {
            return Unauthorized("Invalid Google token");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.GoogleSub == payload.Subject);

        if (user is null)
        {
            user = new User
            {
                GoogleSub = payload.Subject,
                Email = payload.Email,
                Name = payload.Name,
                PictureUrl = payload.Picture,
            };
            _db.Users.Add(user);
        }
        else
        {
            // Keep user info in sync with Google on each login
            user.LastLoginAt = DateTime.UtcNow;
            user.Name = payload.Name;
            user.PictureUrl = payload.Picture;
        }

        await _db.SaveChangesAsync();

        var jwt = _tokenService.GenerateToken(user);
        return Ok(new { token = jwt, name = user.Name, pictureUrl = user.PictureUrl });
    }
}

public record GoogleLoginRequest(string Credential);
