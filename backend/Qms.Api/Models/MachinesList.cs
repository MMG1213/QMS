namespace Qms.Api.Models;

public class MachinesList
{
    public int Id {get; set;}

    public string MachineName {get; set;} = string.Empty;

    public string MachineCategory {get; set;} = string.Empty;

    public string? MachineCodeNum {get; set;}

    public string? MachineSlNum {get; set;}

    public string? Make {get; set;}

    public string CapacityOrSpecification {get; set;} = string.Empty;

    public DateOnly CommissionedOn {get; set;}

    public decimal? LoadInHP {get; set;}

    public string Location {get; set;} = string.Empty;

    public int DocumentTypeId{get;set;}

    public DocumentType?DocumentType{get;set;}


}