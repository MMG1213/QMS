namespace Qms.Api.Models;

public class QualityPerformanceIndex
{
    public int Id {get; set;}

    public string ProcessOrDept {get; set;} = string.Empty;

    public string MeasurementTitle {get; set;} = string.Empty;

    public string QualityPerformanceIndexFormula {get; set;} = string.Empty;

    public string Targets {get; set;} = string.Empty;

    public string Responsibilities {get; set;} = string.Empty;

    public string DepartmentReview {get; set;} = string.Empty;

    public string QPIReview {get; set;} = string.Empty;

    public int DocumentTypeId {get; set;}

    public DocumentType?DocumentType {get; set;}
}