namespace Qms.Api.Models;

public class DepartmentDCRCreator
{
    public int Id { get; set; }

    public int DepartmentApprovalMappingId { get; set; }

    public DepartmentApprovalMapping? DepartmentApprovalMapping { get; set; }

    public string CreatorEmployeeId { get; set; } = string.Empty;

    public AppUser? CreatorEmployee { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}