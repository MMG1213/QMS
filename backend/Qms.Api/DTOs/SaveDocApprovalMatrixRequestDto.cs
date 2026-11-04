
using System.ComponentModel.DataAnnotations;

namespace Qms.Api.DTOs;

public class SaveDocApprovalMatrixRequest
{
    [Required]
    public int DocumentTypeId { get; set; }

    [MaxLength(150)]
    public string? ApprovedBy { get; set; }

    [MaxLength(150)]
    public string? CheckedBy { get; set; }

    [MaxLength(150)]
    public string? IssuedBy { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public DateTime? CheckedDate { get; set; }

    public DateTime? IssuedDate { get; set; }
}
