
using System.ComponentModel.DataAnnotations;

namespace Qms.Api.DTOs;

public class SignupRequestDto
{
    [Required, StringLength(50)]
    public string EmployeeId { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    [Required]
    [RegularExpression(
        "^(User|Department Head)$",
        ErrorMessage = "Role must be User or Department Head.")]
    public string Role { get; set; } = Qms.Api.Models.QmsRole.User;

    [Required]
    [StringLength(128, MinimumLength = 8)]
    [RegularExpression(
        @"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9\s])\S{8,128}$",
        ErrorMessage =
            "Password must be at least 8 characters and contain an uppercase letter, a number, and a special character. Spaces are not allowed.")]
    public string Password { get; set; } = string.Empty;
}
