
namespace Qms.Api.DTOs;

public class AdminSignupRequestDto
{
    public int Id { get; set; }

    public string EmployeeId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    public string Role { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime RequestedAtUtc { get; set; }

    public int? ReviewedByUserId { get; set; }

    public DateTime? ReviewedAtUtc { get; set; }
}
