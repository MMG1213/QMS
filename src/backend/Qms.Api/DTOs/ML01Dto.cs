//RequestDto

public class ML01EntryRequest
{
    public string? Description { get; set; }
    public string? DocumentNo { get; set; }

    public ICollection<ML01RevisionRequest> Revisions { get; set; } = new List<ML01RevisionRequest>();
}

public class ML01RevisionRequest
{
    public int RevisionNo { get; set; }
    public int PageCount { get; set; }
    public DateOnly RevisionDate { get; set; }
}
//Response Dtos;
public class ML01EntryDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public string? DocumentNo { get; set; }

    public ICollection<ML01RevisionDto> Revisions { get; set; } = new List<ML01RevisionDto>();
}

public class ML01RevisionDto
{
    public int Id { get; set; }
     public int RevisionNo { get; set; }
    public int PageCount { get; set; }
    public DateOnly RevisionDate { get; set; }
    
}