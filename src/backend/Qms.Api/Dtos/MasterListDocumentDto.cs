
namespace Qms.Api.Dtos;

public class MasterListDocumentDto
{
     public int DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string DocumentNo { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;

    public string? RetentionPeriod { get; set; }

    public string Status { get; set; } = "Active";

    public DateOnly? ObseleteDate{ get; set; }

    public List<DocumentRevisionDto> Revisions { get; set; } = new();

}
public class DocumentRevisionDto
{
    public int Id{ get; set; }
     public int RevisionNumber { get; set; }

    public int PageCount { get; set; }

    public DateTime RevisionDate { get; set; }
}