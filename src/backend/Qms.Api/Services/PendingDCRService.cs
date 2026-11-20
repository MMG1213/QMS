using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Qms.Api.Data;
using Qms.Api.DTOs;

namespace Qms.Api.Services;

public class PendingDCRService : IPendingDCRService
{
    private readonly QmsDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PendingDCRService(
        QmsDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<DCRPendingDto>> GetPendingDCRsAsync()
    {
        // Get the logged-in user's database ID from the JWT.
        var userIdClaim = _httpContextAccessor.HttpContext?
            .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var currentUserId))
        {
            throw new UnauthorizedAccessException(
                "Unable to identify the logged-in user.");
        }

        // Confirm that the logged-in user is active.
        var currentUserExists = await _context.AppUsers
            .AnyAsync(u => u.Id == currentUserId && u.IsActive);

        if (!currentUserExists)
        {
            throw new UnauthorizedAccessException(
                "The logged-in user is inactive or does not exist.");
        }

        /*
         * Get only stages which:
         * 1. Are assigned to the logged-in user.
         * 2. Are still Pending.
         * 3. Belong to an active workflow.
         * 4. Belong to a submitted or in-progress DCR.
         * 5. Have no earlier Pending stage.
         *
         * This ensures that an approver sees a DCR only when
         * it is actually their turn to approve it.
         */
        var pendingStages = await _context.WorkflowStages
            .Where(stage =>
                stage.ApproverId == currentUserId &&
                stage.Status == "Pending" &&
                stage.WorkFlowInstance != null &&
                stage.WorkFlowInstance.Status == "InProgress" &&
                stage.WorkFlowInstance.DCRRequest != null &&
                (stage.WorkFlowInstance.DCRRequest.Status == "Submitted" ||
                 stage.WorkFlowInstance.DCRRequest.Status == "InProgress") &&

                !_context.WorkflowStages.Any(previousStage =>
                    previousStage.WorkflowInstanceId == stage.WorkflowInstanceId &&
                    previousStage.Status == "Pending" &&
                    previousStage.StageOrder < stage.StageOrder))
            .Include(stage => stage.WorkFlowInstance)
                .ThenInclude(workflow => workflow!.DCRRequest)
                    .ThenInclude(dcr => dcr!.Department)
            .Include(stage => stage.WorkFlowInstance)
                .ThenInclude(workflow => workflow!.DCRRequest)
                    .ThenInclude(dcr => dcr!.DocumentType)
            .Include(stage => stage.WorkFlowInstance)
                .ThenInclude(workflow => workflow!.DCRRequest)
                    .ThenInclude(dcr => dcr!.CreatedByUser)
            .Include(stage => stage.DepartmentApprovalLevel)
            .OrderBy(stage => stage.WorkFlowInstance!.DCRRequest!.RequestDate)
            .ToListAsync();

        var result = new List<DCRPendingDto>();

        foreach (var stage in pendingStages)
        {
            var dcr = stage.WorkFlowInstance!.DCRRequest!;

            // Get all approval actions recorded for this workflow.
            var previousActions = await _context.WorkflowActions
                .Where(action =>
                    action.WorkflowStage != null &&
                    action.WorkflowStage.WorkflowInstanceId ==
                    stage.WorkflowInstanceId)
                .Include(action => action.ActionByUser)
                .Include(action => action.WorkflowStage)
                .OrderBy(action => action.ActionAt)
                .ToListAsync();

            var approvalHistory = previousActions
                .Select(action => new DCRApprovalHistoryDto
                {
                    StageOrder = action.WorkflowStage!.StageOrder,
                    ApproverName = action.ActionByUser!.FullName,
                    Action = action.Action,
                    Comments = action.Comments,
                    ActionAt = action.ActionAt
                })
                .ToList();

            result.Add(new DCRPendingDto
            {
                DCRRequestId = dcr.Id,
                ReferenceNumber = dcr.ReferenceNumber,
                RequestDate = dcr.RequestDate,

                EmployeeName = dcr.CreatedByUser?.FullName ?? string.Empty,
                DepartmentName = dcr.Department?.DepartmentName ?? string.Empty,

                RequestType = dcr.RequestType,
                DocumentTypeName = dcr.DocumentType?.TypeName ?? string.Empty,

                DocumentNo = dcr.DocumentNo,
                DocumentTitle = dcr.DocumentTitle,

                DescriptionBeforechange = dcr.DescriptionBeforechange,
                DescriptionAfterchange = dcr.DescriptionAfterchange,
                Reasonforchange = dcr.Reasonforchange,

                CurrentStageOrder = stage.StageOrder,
                CurrentApprovalType =
                    stage.DepartmentApprovalLevel?.ApprovalType ?? string.Empty,

                ApprovalHistory = approvalHistory
            });
        }

        return result;
    }
}