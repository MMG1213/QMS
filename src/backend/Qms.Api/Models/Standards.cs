namespace Qms.Api.Models;

public class Standards
{
    public int Id {get; set;}

    public string Description {get; set;} = string.Empty;

    public int? Pages {get; set;}

    public int? Revision {get; set;}

    public string? Remarks {get; set;} = string.Empty;

    public int DocumentTypeId{get;set;}

    public DocumentType?DocumentType {get; set;}
}