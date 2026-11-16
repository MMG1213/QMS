namespace Qms.Api.Models;

public class DepartmentApprovalMapping
{
    public int Id { get; set; }

    public int DepartmentId { get; set; }

    public QMSDepartment? QMSDepartment { get; set; }

    public int ConfigVersion { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public string CreatedByEmployeeId { get; set; } = string.Empty;

    public AppUser? CreatedByEmployee { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<DepartmentDCRCreator> DCRCreators { get; set; }
        = new List<DepartmentDCRCreator>();

    public ICollection<DepartmentApprovalLevel> ApprovalLevels { get; set; }
        = new List<DepartmentApprovalLevel>();
}