namespace Qms.Api.Models;

public class WorkflowStage
{
    public int Id { get; set; }

    public int WorkflowInstanceId { get; set; }
    public WorkFlowInstance? WorkFlowInstance { get; set; }

    public int StageOrder { get; set; }

    public int DepartmentApprovalLevelId { get; set; }
    public DepartmentApprovalLevel? DepartmentApprovalLevel { get; set; }

    // Assigned approver
    public int ApproverId { get; set; }
    public AppUser? Approver { get; set; }

    // Stage status
    public string Status { get; set; } = "Pending";

    // Approval or rejection time
    public DateTime? ActionTakenAt { get; set; }

}