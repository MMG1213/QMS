
namespace Qms.Api.Models;

public class DepartmentApprovalMapping
{
    public int Id { get; set; }

    public int DepartmentId { get; set; }

    public QMSDepartment? QMSDepartment { get; set; }

    public string PrimaryApproverEmployeeId { get; set; } = string.Empty;

    public string? RedirectedApproverEmployeeId { get; set; }

    public bool IsActive { get; set; } = true;
}
