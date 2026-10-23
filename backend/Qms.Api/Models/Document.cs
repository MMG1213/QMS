
namespace Qms.Api.Models;

public class Document
{
    public int DocumentId { get; set; }

    public string DocumentNo { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }

    public int DepartmentId { get; set; }
    public QMSDepartment? QMSDepartment { get; set; }

    public string? RetentionPeriod { get; set; }

    public string Status { get; set; } = "Active";

    public ICollection<DocumentRevision> Revisions { get; set; }
        = new List<DocumentRevision>();
}
