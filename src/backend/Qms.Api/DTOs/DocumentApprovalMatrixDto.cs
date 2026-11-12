
namespace Qms.Api.DTOs;

public class DocumentApprovalMatrixDto
{
    public int Id { get; set; }

    public int DocumentTypeId { get; set; }

    public string? ApprovedBy { get; set; }

    public string? CheckedBy { get; set; }

    public string? IssuedBy { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public DateTime? CheckedDate { get; set; }

    public DateTime? IssuedDate { get; set; }
}
