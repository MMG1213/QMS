
namespace Qms.Api.Models;

public class DocumentApprovalMatrix
{
    public int Id { get; set; }

    public string? ApprovedBy { get; set; }

    public string? CheckedBy { get; set; }

    public string? IssuedBy { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public DateTime? CheckedDate { get; set; }

    public DateTime? IssuedDate { get; set; }

    // Foreign key
    public int DocumentTypeId { get; set; }

    // Navigation property
    public DocumentType? DocumentType { get; set; }
}
