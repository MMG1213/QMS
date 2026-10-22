
namespace Qms.Api.Models;

public class DocumentRevision
{
    public int Id { get; set; }

    public int DocumentId { get; set; }

    public Document? Document { get; set; }

    public int RevisionNumber { get; set; }

    public int PageCount { get; set; }

    public DateTime RevisionDate { get; set; }
}
