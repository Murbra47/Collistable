namespace Collistable.Api.Data;

public class User
{
    public int Id { get; set; }

    // Google's stable "sub" claim — never changes even if the user's email changes
    public string GoogleSub { get; set; } = "";
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public string? PictureUrl { get; set; }

    // Auditing
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    public ICollection<Game> Games { get; set; } = new List<Game>();
}
