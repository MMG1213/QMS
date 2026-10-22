namespace Qms.Api.Models;

public class QMSDepartment
{
    public int DepartmentId { get; set; }

    public string DepartmentCode { get; set; } = string.Empty;

    public string DepartmentName { get; set; } = string.Empty;

    public ICollection<Document> Documents { get; set; }
        = new List<Document>();
}