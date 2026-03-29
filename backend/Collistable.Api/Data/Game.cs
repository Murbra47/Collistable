namespace Collistable.Api.Data;

public class Game
{
    public int Id { get; set; }

    // Core metadata
    public string Title { get; set; } = "";
    public string Platform { get; set; } = "";
    public int? ReleaseYear { get; set; }

    // IGDB metadata hooks
    public int? IgdbId { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Summary { get; set; }

    // Collection tracking
    public bool Owned { get; set; } = false;
    public bool Wishlist { get; set; } = false;

    // Auditing
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Ownership — nullable to preserve existing rows during migration
    public int? UserId { get; set; }
    public User? User { get; set; }
}