namespace Qms.Api.DTOs;

public class DCRPendingDto
{
    public int DCRRequestId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateOnly RequestDate { get; set; }

    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;

    public string RequestType { get; set; } = string.Empty;
    public string DocumentTypeName { get; set; } = string.Empty;

    public string? DocumentNo { get; set; }
    public string? DocumentTitle { get; set; }

    public string? DescriptionBeforechange { get; set; }
    public string? DescriptionAfterchange { get; set; }
    public string? Reasonforchange { get; set; }

    public int CurrentStageOrder { get; set; }
    public string CurrentApprovalType { get; set; } = string.Empty;

    public List<DCRApprovalHistoryDto> ApprovalHistory { get; set; } = new();
}

public class DCRApprovalHistoryDto
{
    public int StageOrder { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public DateTime ActionAt { get; set; }
}