
using System.ComponentModel.DataAnnotations;

namespace Qms.Api.DTOs;

public class DCRApprovalDto
{
    [Required]
    [MaxLength(20)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Comments { get; set; }
}
