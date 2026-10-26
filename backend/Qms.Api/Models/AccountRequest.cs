
namespace Qms.Api.Models;

public static class AccountRequestStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}

public class AccountRequest
{
    public int Id { get; set; }

    public string EmployeeId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public Department? Department { get; set; }

    // The role requested by the employee.
    public string Role { get; set; } = QmsRole.User;

    // Signup requests can only be Pending, Approved, or Rejected.
    public string Status { get; set; } = AccountRequestStatus.Pending;

    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;

    public int? ReviewedByUserId { get; set; }

    public AppUser? ReviewedByUser { get; set; }

    public DateTime? ReviewedAtUtc { get; set; }

    // Store only the password hash, never the plaintext password.
    // Nullable so existing requests remain compatible.
    public string? PasswordHash { get; set; }

    // Legacy password-setup fields retained for compatibility.
    public string? PasswordSetupTokenHash { get; set; }

    public DateTime? PasswordSetupTokenExpiresAtUtc { get; set; }
}
