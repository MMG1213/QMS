
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

    // AccountRequests now references dbo.QMSDepartments.
    public QMSDepartment? QMSDepartment { get; set; }

    public string Role { get; set; } = QmsRole.User;

    public string Status { get; set; } = AccountRequestStatus.Pending;

    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;

    public int? ReviewedByUserId { get; set; }

    public AppUser? ReviewedByUser { get; set; }

    public DateTime? ReviewedAtUtc { get; set; }

    public string? PasswordHash { get; set; }

    public string? PasswordSetupTokenHash { get; set; }

    public DateTime? PasswordSetupTokenExpiresAtUtc { get; set; }
}
