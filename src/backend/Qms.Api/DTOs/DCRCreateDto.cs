
using System.ComponentModel.DataAnnotations;

namespace Qms.Api.DTOs;

public class DCRCreateDto
{
    [Required]
    public DateOnly RequestDate { get; set; }

    [Required]
    [MaxLength(100)]
    public string Standard { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string RequestType { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string DocumentTypeName { get; set; } = string.Empty;

    public int? DocumentId { get; set; }

    [MaxLength(250)]
    public string? DocumentTitle { get; set; }

    [MaxLength(100)]
    public string? DocumentNo { get; set; }

    [MaxLength(2000)]
    public string? DescriptionBeforechange { get; set; }

    [MaxLength(2000)]
    public string? DescriptionAfterchange { get; set; }

    [MaxLength(2000)]
    public string? Reasonforchange { get; set; }
}
