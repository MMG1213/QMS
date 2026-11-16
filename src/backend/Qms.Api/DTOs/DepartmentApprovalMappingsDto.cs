namespace Qms.Api.DTOs.DepartmentApprovalMappings;

public class DepartmentApprovalMappingDto
{
    public int Id { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int ConfigVersion { get; set; }
    public bool IsActive { get; set; }

    public string CreatedByEmployeeId { get; set; } = string.Empty;
    public string CreatedByEmployeeName { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public List<DCRCreatorDto> DCRCreators { get; set; } = new();
    public List<ApprovalLevelDto> ApprovalLevels { get; set; } = new();
}

public class DCRCreatorDto
{
    public string CreatorEmployeeId { get; set; } = string.Empty;
    public string CreatorEmployeeName { get; set; } = string.Empty;
}

public class ApprovalLevelDto
{
    public int Id { get; set; }
    public int LevelNumber { get; set; }

    // Department or QMSFinal
    public string ApprovalType { get; set; } = "Department";

    public string ApproverEmployeeId { get; set; } = string.Empty;
    public string ApproverEmployeeName { get; set; } = string.Empty;

    public string? RedirectedApproverEmployeeId { get; set; }
    public string? RedirectedApproverEmployeeName { get; set; }

    public bool IsActive { get; set; }
}

public class CreateDepartmentApprovalMappingDto
{
    public int DepartmentId { get; set; }

    public List<string> CreatorEmployeeIds { get; set; } = new();

    public List<CreateApprovalLevelDto> ApprovalLevels { get; set; } = new();
}

public class UpdateDepartmentApprovalMappingDto
{
    public bool IsActive { get; set; }

    public List<string> CreatorEmployeeIds { get; set; } = new();

    public List<CreateApprovalLevelDto> ApprovalLevels { get; set; } = new();
}

public class CreateApprovalLevelDto
{
    public int LevelNumber { get; set; }

    public string ApprovalType { get; set; } = "Department";

    public string ApproverEmployeeId { get; set; } = string.Empty;

    public string? RedirectedApproverEmployeeId { get; set; }

    public bool IsActive { get; set; } = true;
}

public class ApprovalMappingEmployeeDto
{
    public string EmployeeId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
}