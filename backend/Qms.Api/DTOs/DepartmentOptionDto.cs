
namespace Qms.Api.DTOs;

public class DepartmentOptionDto
{
    public int Id { get; set; }

    public string DeptName { get; set; } = string.Empty;

    public string DeptId { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
