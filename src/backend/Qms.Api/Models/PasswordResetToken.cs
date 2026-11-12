
namespace Qms.Api.Models;

public class PasswordResetToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public AppUser? User { get; set; }

    // Store only the SHA-256 hash of the random reset token.
    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? UsedAtUtc { get; set; }
}
