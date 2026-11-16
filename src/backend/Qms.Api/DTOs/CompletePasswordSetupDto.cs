using System.ComponentModel.DataAnnotations;

namespace Qms.Api.DTOs;

public class CompletePasswordSetupDto
{
    [Required]
    [StringLength(200, MinimumLength = 20)]
    public string Token { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 12)]
    public string NewPassword { get; set; } = string.Empty;
}