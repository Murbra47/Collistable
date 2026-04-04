using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    private static readonly JsonSerializerOptions CaseInsensitive = new() { PropertyNameCaseInsensitive = true };

    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(AppDbContext db, TokenService tokenService, IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _tokenService = tokenService;
        _config = config;
        _httpClientFactory = httpClientFactory;
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
            // Check if an account with this email already exists (e.g. from GitHub login) — link it
            user = await _db.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);
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
                user.GoogleSub = payload.Subject;
                user.LastLoginAt = DateTime.UtcNow;
                user.Name = payload.Name;
                user.PictureUrl = payload.Picture;
            }
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

    // POST: api/auth/github
    [HttpPost("github")]
    public async Task<IActionResult> GithubLogin([FromBody] GithubLoginRequest request)
    {
        var client = _httpClientFactory.CreateClient("github");

        var accessToken = await ExchangeGithubCode(client, request.Code);
        if (accessToken is null)
            return Unauthorized("Failed to exchange GitHub code");

        var profile = await FetchGithubProfile(client, accessToken);
        if (profile is null)
            return Unauthorized("Failed to fetch GitHub profile");

        // GitHub profile email can be null if the user set it to private — fall back to the emails API
        var email = profile.Email ?? await FetchGithubPrimaryEmail(client, accessToken);
        if (email is null)
            return BadRequest("No accessible email on this GitHub account");

        var githubId = profile.Id.ToString();

        // Find by GitHub ID first, then by email to link an existing account
        var user = await _db.Users.FirstOrDefaultAsync(u => u.GithubId == githubId)
                   ?? await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
        {
            user = new User
            {
                GithubId = githubId,
                Email = email,
                Name = profile.Name ?? profile.Login,
                PictureUrl = profile.AvatarUrl,
            };
            _db.Users.Add(user);
        }
        else
        {
            user.GithubId = githubId;
            user.LastLoginAt = DateTime.UtcNow;
            user.Name = profile.Name ?? profile.Login;
            user.PictureUrl = profile.AvatarUrl;
        }

        await _db.SaveChangesAsync();

        var jwt = _tokenService.GenerateToken(user);
        return Ok(new { token = jwt, name = user.Name, pictureUrl = user.PictureUrl });
    }

    private async Task<string?> ExchangeGithubCode(HttpClient client, string code)
    {
        var response = await client.PostAsJsonAsync(
            "https://github.com/login/oauth/access_token",
            new
            {
                client_id = _config["Github:ClientId"],
                client_secret = _config["Github:ClientSecret"],
                code,
            });

        if (!response.IsSuccessStatusCode) return null;

        var data = await response.Content.ReadFromJsonAsync<JsonElement>();
        return data.TryGetProperty("access_token", out var token) ? token.GetString() : null;
    }

    private static async Task<GithubUserProfile?> FetchGithubProfile(HttpClient client, string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<GithubUserProfile>(CaseInsensitive);
    }

    private static async Task<string?> FetchGithubPrimaryEmail(HttpClient client, string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        var emails = await response.Content.ReadFromJsonAsync<List<GithubEmail>>(CaseInsensitive);
        return emails?.FirstOrDefault(e => e.Primary && e.Verified)?.Email;
    }
}

public record GoogleLoginRequest(string Credential);
public record GithubLoginRequest(string Code);

public class GithubUserProfile
{
    public long Id { get; set; }
    public string Login { get; set; } = "";
    public string? Name { get; set; }
    public string? Email { get; set; }
    [JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }
}

public class GithubEmail
{
    public string Email { get; set; } = "";
    public bool Primary { get; set; }
    public bool Verified { get; set; }
}
