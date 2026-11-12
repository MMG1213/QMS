
using System.ComponentModel.DataAnnotations;

namespace Qms.Api.DTOs;

public class CreateDepartmentRequest
{
    [Required]
    [StringLength(20)]
    public string DepartmentCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string DepartmentName { get; set; } = string.Empty;
}
