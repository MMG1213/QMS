
using System.ComponentModel.DataAnnotations;

namespace Qms.Api.DTOs;

public class ReviewSignupRequestDto
{
    [Required]
    public string Decision { get; set; } = string.Empty;
}
