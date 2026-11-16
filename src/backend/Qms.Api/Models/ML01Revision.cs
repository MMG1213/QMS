namespace Qms.Api.Models;

public class ML01Revision
{
    public int Id { get; set; }
    public int ML01EntryId { get; set; }

    public int RevisionNo { get; set; }

    public int PageCount { get; set; }

    public DateOnly RevisionDate { get; set; }

    public ML01Entry ML01Entry { get; set; } = null!;
}