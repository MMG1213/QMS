
using System.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Qms.Api.Data;
using Qms.Api.DTOs;
using Qms.Api.Models;

namespace Qms.Api.Services;

public class WorkflowApprovalService : IWorkflowApprovalService
{
    private readonly QmsDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WorkflowApprovalService(
        QmsDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task ApproveOrRejectAsync(
        int dcrRequestId,
        DCRApprovalDto request)
    {
        // 1. Identify the logged-in approver.
        var userIdClaim = _httpContextAccessor.HttpContext?
            .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException(
                "Invalid user identity.");
        }

        // 2. Validate the requested action.
        string action = request.Action.Trim();

        if (!string.Equals(action, "Approved",
                StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(action, "Rejected",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Action must be Approved or Rejected.");
        }

        action = action.Equals(
            "Approved",
            StringComparison.OrdinalIgnoreCase)
                ? "Approved"
                : "Rejected";

        if (action == "Rejected" &&
            string.IsNullOrWhiteSpace(request.Comments))
        {
            throw new InvalidOperationException(
                "A rejection reason is required.");
        }

        // 3. Start transaction.
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            DateTime indiaTime =
                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                    DateTime.UtcNow,
                    "India Standard Time");

            // 4. Retrieve active logged-in approver.
            var approver = await _context.AppUsers
                .FirstOrDefaultAsync(u =>
                    u.Id == userId &&
                    u.IsActive);

            if (approver == null)
            {
                throw new UnauthorizedAccessException(
                    "Approver not found or inactive.");
            }

            // 5. Retrieve DCR.
            var dcr = await _context.DCRRequests
                .FirstOrDefaultAsync(d => d.Id == dcrRequestId);

            if (dcr == null)
            {
                throw new KeyNotFoundException(
                    "DCR request not found.");
            }

            if (dcr.Status != "Submitted" &&
                dcr.Status != "InProgress")
            {
                throw new InvalidOperationException(
                    "This DCR is no longer awaiting approval.");
            }

            // 6. Retrieve workflow instance.
            var workflow = await _context.WorkFlowInstances
                .FirstOrDefaultAsync(w =>
                    w.DCRRequestId == dcrRequestId);

            if (workflow == null)
            {
                throw new InvalidOperationException(
                    "Workflow instance not found.");
            }

            if (workflow.Status != "InProgress")
            {
                throw new InvalidOperationException(
                    "This workflow is already completed.");
            }

            // 7. Retrieve workflow stages in sequence.
            var stages = await _context.WorkflowStages
                .Where(s => s.WorkflowInstanceId == workflow.Id)
                .OrderBy(s => s.StageOrder)
                .ToListAsync();

            if (stages.Count == 0)
            {
                throw new InvalidOperationException(
                    "No workflow stages were found.");
            }

            // 8. Find the current pending stage.
            var currentStage = stages
                .FirstOrDefault(s => s.Status == "Pending");

            if (currentStage == null)
            {
                throw new InvalidOperationException(
                    "There are no pending approval stages.");
            }

            // 9. Verify that the logged-in user is
            // the approver assigned to this stage.
            if (currentStage.ApproverId != approver.Id)
            {
                throw new UnauthorizedAccessException(
                    "You are not the assigned approver for the current stage.");
            }

            // 10. Update the current workflow stage.
            currentStage.Status = action;
            currentStage.ActionTakenAt = indiaTime;

            // 11. Record the approval/rejection history.
            var workflowAction = new WorkflowAction
            {
                WorkflowStageId = currentStage.Id,
                ActionByUserId = approver.Id,
                Action = action,
                Comments = string.IsNullOrWhiteSpace(request.Comments)
                    ? null
                    : request.Comments.Trim(),
                ActionAt = indiaTime
            };

            _context.WorkflowActions.Add(workflowAction);

            // 12. If rejected, stop the workflow.
            if (action == "Rejected")
            {
                dcr.Status = "Rejected";
                dcr.RejectionReason = request.Comments!.Trim();
                dcr.UpdatedAt = indiaTime;

                workflow.Status = "Rejected";
                workflow.CompletedAt = indiaTime;

                // Mark remaining pending stages as skipped.
                foreach (var stage in stages.Where(s =>
                    s.StageOrder > currentStage.StageOrder &&
                    s.Status == "Pending"))
                {
                    stage.Status = "Skipped";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return;
            }

            // 13. Check whether another approval stage exists.
            var nextStage = stages
                .FirstOrDefault(s =>
                    s.StageOrder > currentStage.StageOrder &&
                    s.Status == "Pending");

            if (nextStage != null)
            {
                // More approval stages remain.
                dcr.Status = "InProgress";
                dcr.UpdatedAt = indiaTime;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return;
            }

            // 14. All approval stages are complete.
            // Perform the final document operation.
            await FinalizeDCRAsync(dcr, indiaTime);

            // 15. Complete workflow and DCR.
            dcr.Status = "Approved";
            dcr.FinalizedAt = indiaTime;
            dcr.UpdatedAt = indiaTime;

            workflow.Status = "Completed";
            workflow.CompletedAt = indiaTime;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task FinalizeDCRAsync(
        DCRRequest dcr,
        DateTime indiaTime)
    {
        // ADDITION
        if (dcr.RequestType == "Addition")
        {
            if (string.IsNullOrWhiteSpace(dcr.DocumentNo) ||
                string.IsNullOrWhiteSpace(dcr.DocumentTitle))
            {
                throw new InvalidOperationException(
                    "Document number and title are required.");
            }

            bool documentExists = await _context.Documents
                .AnyAsync(d => d.DocumentNo == dcr.DocumentNo);

            if (documentExists)
            {
                throw new InvalidOperationException(
                    "This document number already exists.");
            }

            var document = new Document
            {
                DocumentNo = dcr.DocumentNo,
                Title = dcr.DocumentTitle,
                DocumentTypeId = dcr.DocumentTypeId,
                DepartmentId = dcr.DepartmentId,
                RetentionPeriod = null,
                Status = "Active",
                ObseleteDate = null
            };

            _context.Documents.Add(document);

            // Save to generate DocumentId.
            await _context.SaveChangesAsync();

            // Create initial revision.
            var initialRevision = new DocumentRevision
            {
                DocumentId = document.DocumentId,
                RevisionNumber = dcr.NewRevisionNumber ?? 0,
                PageCount = 0,
                RevisionDate = indiaTime
            };

            _context.DocumentRevisions.Add(initialRevision);

            // Link the newly created document to the DCR.
            dcr.DocumentId = document.DocumentId;
        }

        // REVISION
        else if (dcr.RequestType == "Revision")
        {
            if (!dcr.DocumentId.HasValue)
            {
                throw new InvalidOperationException(
                    "Document ID is missing for this revision.");
            }

            var document = await _context.Documents
                .Include(d => d.Revisions)
                .FirstOrDefaultAsync(d =>
                    d.DocumentId == dcr.DocumentId.Value);

            if (document == null)
            {
                throw new InvalidOperationException(
                    "Document not found.");
            }

            if (document.Status != "Active")
            {
                throw new InvalidOperationException(
                    "Cannot revise an obsolete document.");
            }

            // Ensure no newer revision was added after submission.
            int actualCurrentRevision = document.Revisions
                .Select(r => r.RevisionNumber)
                .DefaultIfEmpty(0)
                .Max();

            if (dcr.CurrentRevisionNumber != actualCurrentRevision)
            {
                throw new InvalidOperationException(
                    "The document revision has changed since this DCR was submitted. Please raise a new DCR using the latest revision.");
            }

            int newRevisionNumber =
                dcr.NewRevisionNumber ??
                (actualCurrentRevision + 1);

            if (newRevisionNumber != actualCurrentRevision + 1)
            {
                throw new InvalidOperationException(
                    "Invalid new revision number.");
            }

            var newRevision = new DocumentRevision
            {
                DocumentId = document.DocumentId,
                RevisionNumber = newRevisionNumber,
                PageCount = 0,
                RevisionDate = indiaTime
            };

            _context.DocumentRevisions.Add(newRevision);
        }

        // OBSOLETE
        else if (dcr.RequestType == "Obsolete")
        {
            if (!dcr.DocumentId.HasValue)
            {
                throw new InvalidOperationException(
                    "Document ID is missing for this obsolete request.");
            }

            var document = await _context.Documents
                .FirstOrDefaultAsync(d =>
                    d.DocumentId == dcr.DocumentId.Value);

            if (document == null)
            {
                throw new InvalidOperationException(
                    "Document not found.");
            }

            if (document.Status != "Active")
            {
                throw new InvalidOperationException(
                    "This document is already obsolete.");
            }

            document.Status = "Obsolete";

            document.ObseleteDate =
                DateOnly.FromDateTime(indiaTime);
        }
        else
        {
            throw new InvalidOperationException(
                "Invalid DCR request type.");
        }
    }
}
