
using System.ComponentModel.DataAnnotations;

namespace Qms.Api.DTOs;

public class AddDocumentDto
{
    [Required]
    public string DocumentNo { get; set; } = string.Empty;

    [Required]
    public string Title { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int DocumentTypeId { get; set; }

    [Range(1, int.MaxValue)]
    public int DepartmentId { get; set; }

    [StringLength(100)]
    public string? Standard { get; set; }

    public string? RetentionPeriod { get; set; }

    [Range(1, int.MaxValue)]
    public int PageCount { get; set; } = 1;

    public DateTime RevisionDate { get; set; } = DateTime.Today;
}
