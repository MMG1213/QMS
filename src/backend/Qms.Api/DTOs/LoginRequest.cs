
using System.ComponentModel.DataAnnotations;

namespace Qms.Api.DTOs;

public class LoginRequest
{
    [Required]
    [StringLength(256)]
    public string LoginId { get; set; } = string.Empty;

    [Required]
    [StringLength(128, MinimumLength = 1)]
    public string Password { get; set; } = string.Empty;
}
