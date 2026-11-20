namespace Qms.Api.Models;

public class WorkflowAction
{
    public int Id { get; set; }

    // Stage associated with this action
    public int WorkflowStageId { get; set; }
    public WorkflowStage? WorkflowStage { get; set; }

    // Approver who performed the action
    public int ActionByUserId { get; set; }
    public AppUser? ActionByUser { get; set; }

    // Approved or Rejected
    public string Action { get; set; } = string.Empty;

    // Approver comments or rejection reason
    public string? Comments { get; set; }

    // When the action happened
    public DateTime ActionAt { get; set; } =
        TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
            DateTime.UtcNow,
            "India Standard Time"
        );
}