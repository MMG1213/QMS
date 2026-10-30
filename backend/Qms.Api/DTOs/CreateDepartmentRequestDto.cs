
using System.ComponentModel.DataAnnotations;

namespace Qms.Api.DTOs;

public class CreateDepartmentRequest
{
    [Required]
    [StringLength(100)]
    public string DeptName { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string DeptId { get; set; } = string.Empty;
}
