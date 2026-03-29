using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Collistable.Api.Services;

public class IGDBService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public IGDBService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    private async Task<string> GetTokenAsync()
    {
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiry)
            return _cachedToken;

        string? clientId = _config["TWITCH_CLIENT_ID"];
        string? clientSecret = _config["TWITCH_CLIENT_SECRET"];

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new InvalidOperationException("Missing IGDB/Twitch API credentials. Ensure TWITCH_CLIENT_ID and TWITCH_CLIENT_SECRET are set.");
        }

        var tokenRes = await _http.PostAsync(
            $"https://id.twitch.tv/oauth2/token?client_id={clientId!}&client_secret={clientSecret!}&grant_type=client_credentials",
            null
        );

        var tokenJson = await tokenRes.Content.ReadAsStringAsync();
        var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenJson);

        _cachedToken = tokenData.GetProperty("access_token").GetString();
        int expires = tokenData.GetProperty("expires_in").GetInt32();
        _tokenExpiry = DateTime.UtcNow.AddSeconds(expires - 60);

        return _cachedToken!;
    }

    public async Task<string> SearchGamesAsync(string query)
    {
        var token = await GetTokenAsync();
        string? clientId = _config["TWITCH_CLIENT_ID"];

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new InvalidOperationException("Missing IGDB/Twitch API credentials. Ensure TWITCH_CLIENT_ID is set.");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.igdb.com/v4/games");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("Client-ID", clientId);

        string body = $@"
            search ""{query}"";
            fields id, name, summary, first_release_date, cover.image_id, platforms.name;
            limit 20;
        ";

        request.Content = new StringContent(body, Encoding.UTF8, "text/plain");

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}