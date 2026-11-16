namespace Qms.Api.Models;

public class DepartmentApprovalLevel
{
    public int Id { get; set; }

    public int DepartmentApprovalMappingId { get; set; }

    public DepartmentApprovalMapping? DepartmentApprovalMapping { get; set; }

    public int LevelNumber { get; set; }

    public string ApprovalType { get; set; } = "Department";

    public string ApproverEmployeeId { get; set; } = string.Empty;

    public AppUser? ApproverEmployee { get; set; }

    public string? RedirectedApproverEmployeeId { get; set; }

    public AppUser? RedirectedApproverEmployee { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}