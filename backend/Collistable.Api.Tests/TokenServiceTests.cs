using System.IdentityModel.Tokens.Jwt;
using Collistable.Api.Data;
using Collistable.Api.Services;
using Microsoft.Extensions.Configuration;

namespace Collistable.Api.Tests;

public class TokenServiceTests
{
    private static TokenService CreateService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-secret-that-is-long-enough-for-hmac-sha256",
                ["Jwt:Issuer"] = "Collistable",
                ["Jwt:Audience"] = "Collistable",
            })
            .Build();

        return new TokenService(config);
    }

    private static JwtSecurityToken Decode(string token)
        => new JwtSecurityTokenHandler().ReadJwtToken(token);

    [Fact]
    public void GenerateToken_ContainsSubClaim()
    {
        var service = CreateService();
        var user = new User { Id = 7, Email = "test@example.com", Name = "Test User" };

        var token = Decode(service.GenerateToken(user));

        Assert.Equal("7", token.Subject);
    }

    [Fact]
    public void GenerateToken_ContainsEmailClaim()
    {
        var service = CreateService();
        var user = new User { Id = 1, Email = "test@example.com", Name = "Test User" };

        var token = Decode(service.GenerateToken(user));

        Assert.Equal("test@example.com", token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
    }

    [Fact]
    public void GenerateToken_ContainsNameClaim()
    {
        var service = CreateService();
        var user = new User { Id = 1, Email = "test@example.com", Name = "Test User" };

        var token = Decode(service.GenerateToken(user));

        Assert.Equal("Test User", token.Claims.First(c => c.Type == "name").Value);
    }

    [Fact]
    public void GenerateToken_ContainsPictureClaim_WhenPictureUrlIsSet()
    {
        var service = CreateService();
        var user = new User { Id = 1, Email = "test@example.com", Name = "Test User", PictureUrl = "https://example.com/pic.jpg" };

        var token = Decode(service.GenerateToken(user));

        Assert.Equal("https://example.com/pic.jpg", token.Claims.First(c => c.Type == "picture").Value);
    }

    [Fact]
    public void GenerateToken_OmitsPictureClaim_WhenPictureUrlIsNull()
    {
        var service = CreateService();
        var user = new User { Id = 1, Email = "test@example.com", Name = "Test User", PictureUrl = null };

        var token = Decode(service.GenerateToken(user));

        Assert.DoesNotContain(token.Claims, c => c.Type == "picture");
    }

    [Fact]
    public void GenerateToken_ExpiresInSevenDays()
    {
        var service = CreateService();
        var user = new User { Id = 1, Email = "test@example.com", Name = "Test User" };
        var before = DateTime.UtcNow.AddDays(7);

        var token = Decode(service.GenerateToken(user));

        Assert.True(token.ValidTo >= before.AddMinutes(-1));
        Assert.True(token.ValidTo <= before.AddMinutes(1));
    }
}
