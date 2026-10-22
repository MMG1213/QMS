namespace Qms.Api.Models;

public class DocumentType
{
    public int DocumentTypeId { get; set; }

    public string TypeCode { get; set; } = string.Empty;

    public string TypeName { get; set; } = string.Empty;

    // Navigation property
    public ICollection<Document> Documents { get; set; }
        = new List<Document>();
}