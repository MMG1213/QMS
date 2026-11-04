namespace Qms.Api.Models;

public class ML01Entry
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;

    public string DocumentNo { get; set; } = string.Empty;
    public int DocumentTypeId{ get; set; }

    public DocumentType DocumentType { get; set; } = null !;

    public ICollection<ML01Revision> Revisions{ get; set; } = new List<ML01Revision>();
}