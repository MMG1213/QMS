namespace Qms.Api.Models;

public class WorkFlowInstance
{
    public int Id { get; set; }

    public int DCRRequestId { get; set; }

    public DCRRequest? DCRRequest { get; set; }

    public int DepartmentApprovalMappingId { get; set; }
    public DepartmentApprovalMapping? DepartmentApprovalMapping { get; set; }

    public string Status { get; set; } = "InProgress";

    public DateTime StartedAt { get; set; } = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "India Standard Time");

    public DateTime?CompletedAt{ get; set; }
    
}