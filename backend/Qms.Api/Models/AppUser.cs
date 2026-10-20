
namespace Qms.Api.Models;

public class AppUser
{
    public int Id { get; set; }

    public string EmployeeId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public Department? Department { get; set; }

    public bool IsActive { get; set; } = true;
}
