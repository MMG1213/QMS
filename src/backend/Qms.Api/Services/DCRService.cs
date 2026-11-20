
using System.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Qms.Api.Data;
using Qms.Api.DTOs;
using Qms.Api.Models;

namespace Qms.Api.Services;

public class DCRService : IDCRService
{
    private readonly QmsDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DCRService(
        QmsDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> CreateDCRAsync(DCRCreateDto request)
    {
        // 1. Get logged-in employee ID from JWT.
        var userIdClaim = _httpContextAccessor.HttpContext?
            .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException(
                "Invalid user identity.");
        }

        // 2. Start a serializable transaction.
        // This protects sequential number generation from concurrent requests.
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            // 3. Retrieve logged-in employee.
            var currentUser = await _context.AppUsers
                .FirstOrDefaultAsync(u =>
                    u.Id == userId &&
                    u.IsActive);

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException(
                    "User not found or inactive.");
            }

            if (!currentUser.DepartmentId.HasValue)
            {
                throw new InvalidOperationException(
                    "Employee does not belong to any department.");
            }

            int departmentId = currentUser.DepartmentId.Value;

            // 4. Retrieve department.
            var department = await _context.QMSDepartments
                .FirstOrDefaultAsync(d =>
                    d.DepartmentId == departmentId &&
                    d.IsActive);

            if (department == null)
            {
                throw new InvalidOperationException(
                    "Department not found or inactive.");
            }

            if (string.IsNullOrWhiteSpace(department.DepartmentCode))
            {
                throw new InvalidOperationException(
                    "Department code is not configured.");
            }

            string departmentCode = department.DepartmentCode.Trim();

            // 5. Validate request type.
            string requestType = request.RequestType.Trim();

            if (requestType != "Addition" &&
                requestType != "Revision" &&
                requestType != "Obsolete")
            {
                throw new InvalidOperationException(
                    "Invalid DCR request type.");
            }

            // 6. Retrieve active approval mapping.
            var mapping = await _context.DepartmentApprovalMappings
                .Include(m => m.DCRCreators)
                .Include(m => m.ApprovalLevels)
                .FirstOrDefaultAsync(m =>
                    m.DepartmentId == departmentId &&
                    m.IsActive);

            if (mapping == null)
            {
                throw new InvalidOperationException(
                    "No active approval mapping is configured for this department.");
            }

            // 7. Verify employee is authorized to raise DCR.
            bool isAuthorizedCreator = mapping.DCRCreators.Any(c =>
                c.CreatorEmployeeId == currentUser.EmployeeId);

            if (!isAuthorizedCreator)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to raise DCR requests.");
            }

            // 8. Retrieve Department Head as Process Owner.
            var departmentHeads = await _context.AppUsers
                .Where(u =>
                    u.DepartmentId == departmentId &&
                    u.Role == QmsRole.DepartmentHead &&
                    u.IsActive)
                .ToListAsync();

            if (departmentHeads.Count == 0)
            {
                throw new InvalidOperationException(
                    "No active Department Head is configured for this department.");
            }

            if (departmentHeads.Count > 1)
            {
                throw new InvalidOperationException(
                    "Multiple active Department Heads were found.");
            }

            var departmentHead = departmentHeads[0];

            // 9. Retrieve document type.
            DocumentType? documentType = null;
            Document? existingDocument = null;

            int? currentRevisionNumber = null;
            int? newRevisionNumber = null;

            if (requestType == "Addition")
            {
                if (string.IsNullOrWhiteSpace(request.DocumentTypeName))
                {
                    throw new InvalidOperationException(
                        "Document type is required.");
                }

                var typeName = request.DocumentTypeName.Trim();

                var matchingTypes = await _context.DocumentTypes
                    .Where(t =>
                        t.TypeName == typeName &&
                        t.IsActive)
                    .ToListAsync();

                if (matchingTypes.Count == 0)
                {
                    throw new InvalidOperationException(
                        "Selected document type does not exist or is inactive.");
                }

                if (matchingTypes.Count > 1)
                {
                    throw new InvalidOperationException(
                        "Duplicate document type names exist.");
                }

                documentType = matchingTypes[0];

                if (string.IsNullOrWhiteSpace(documentType.DocumentTypeCode))
                {
                    throw new InvalidOperationException(
                        "Document type code is not configured for this document type.");
                }
            }

            // 10. Retrieve existing document for Revision or Obsolete.
            if (requestType == "Revision" ||
                requestType == "Obsolete")
            {
                if (!request.DocumentId.HasValue)
                {
                    throw new InvalidOperationException(
                        "Please select an existing document.");
                }

                existingDocument = await _context.Documents
                    .Include(d => d.Revisions)
                    .FirstOrDefaultAsync(d =>
                        d.DocumentId == request.DocumentId.Value);

                if (existingDocument == null)
                {
                    throw new InvalidOperationException(
                        "Selected document does not exist.");
                }

                if (existingDocument.DepartmentId != departmentId)
                {
                    throw new UnauthorizedAccessException(
                        "You can only raise DCR requests for documents in your department.");
                }

                if (existingDocument.Status != "Active")
                {
                    throw new InvalidOperationException(
                        "Only active documents can be selected.");
                }

                documentType = await _context.DocumentTypes
                    .FirstOrDefaultAsync(t =>
                        t.DocumentTypeId == existingDocument.DocumentTypeId);

                if (documentType == null)
                {
                    throw new InvalidOperationException(
                        "Document type associated with the selected document was not found.");
                }

                // For Revision, calculate the next revision number.
                if (requestType == "Revision")
                {
                    currentRevisionNumber = existingDocument.Revisions
                        .Select(r => r.RevisionNumber)
                        .DefaultIfEmpty(0)
                        .Max();

                    newRevisionNumber = currentRevisionNumber.Value + 1;
                }
            }

            // 11. Generate or retrieve DocumentNo.
            string documentNo;

            if (requestType == "Addition")
            {
                string typeCode = documentType!.DocumentTypeCode!.Trim();

                string documentPrefix =
                    $"{departmentCode}-{typeCode}-";

                var existingDocumentNumbers = await _context.Documents
                    .Where(d => d.DocumentNo.StartsWith(documentPrefix))
                    .Select(d => d.DocumentNo)
                    .ToListAsync();

                if (existingDocumentNumbers.Count == 0)
                {
                    // First document for this department + document type.
                    // Employee must provide the initial document number.
                    if (string.IsNullOrWhiteSpace(request.DocumentNo))
                    {
                        throw new InvalidOperationException(
                            $"Enter the first document number using the format {documentPrefix}001.");
                    }

                    documentNo = request.DocumentNo.Trim();

                    if (!documentNo.StartsWith(
                        documentPrefix,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            $"Document number must start with {documentPrefix}");
                    }

                    string initialSequence =
                        documentNo.Substring(documentPrefix.Length);

                    if (!int.TryParse(initialSequence, out int initialNumber) ||
                        initialNumber < 1 ||
                        initialSequence.Length != 3)
                    {
                        throw new InvalidOperationException(
                            "Document number must end with a three-digit sequence, such as 001.");
                    }

                    bool duplicateNumber = await _context.Documents
                        .AnyAsync(d => d.DocumentNo == documentNo);

                    if (duplicateNumber)
                    {
                        throw new InvalidOperationException(
                            "This document number already exists.");
                    }
                }
                else
                {
                    // Subsequent documents are numbered automatically.
                    var sequenceNumbers = existingDocumentNumbers
                        .Select(number =>
                        {
                            string sequence =
                                number.Substring(documentPrefix.Length);

                            return int.TryParse(sequence, out int value)
                                ? value
                                : 0;
                        });

                    int nextNumber = sequenceNumbers.DefaultIfEmpty(0).Max() + 1;

                    if (nextNumber > 999)
                    {
                        throw new InvalidOperationException(
                            "Document number sequence has exceeded 999.");
                    }

                    documentNo = $"{documentPrefix}{nextNumber:D3}";
                }

                if (string.IsNullOrWhiteSpace(request.DocumentTitle))
                {
                    throw new InvalidOperationException(
                        "Document title is required for Addition.");
                }
            }
            else
            {
                // Revision and Obsolete use the existing document number.
                documentNo = existingDocument!.DocumentNo;
            }

            // 12. Retrieve active approval levels.
            var approvalLevels = mapping.ApprovalLevels
                .Where(l => l.IsActive)
                .OrderBy(l => l.LevelNumber)
                .ToList();

            if (approvalLevels.Count == 0)
            {
                throw new InvalidOperationException(
                    "No active approval stages are configured.");
            }

            // 13. Resolve approver EmployeeIds to AppUser IDs.
            var approverEmployeeIds = approvalLevels
                .Select(l => l.ApproverEmployeeId)
                .Distinct()
                .ToList();

            var approvers = await _context.AppUsers
                .Where(u =>
                    approverEmployeeIds.Contains(u.EmployeeId) &&
                    u.IsActive)
                .ToListAsync();

            var approverLookup = approvers.ToDictionary(
                u => u.EmployeeId,
                u => u.Id);

            foreach (var level in approvalLevels)
            {
                if (string.IsNullOrWhiteSpace(level.ApproverEmployeeId) ||
                    !approverLookup.ContainsKey(level.ApproverEmployeeId))
                {
                    throw new InvalidOperationException(
                        $"Approver for approval level {level.LevelNumber} is missing or inactive.");
                }
            }

            // 14. Generate yearly DCR reference number.
            DateTime indiaTime =
                TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
                    DateTime.UtcNow,
                    "India Standard Time");

            int currentYear = indiaTime.Year;

            string referencePrefix = $"{currentYear}_";

            var existingReferences = await _context.DCRRequests
                .Where(d => d.ReferenceNumber.StartsWith(referencePrefix))
                .Select(d => d.ReferenceNumber)
                .ToListAsync();

            int nextDcrNumber = existingReferences
                .Select(reference =>
                {
                    string sequence =
                        reference.Substring(referencePrefix.Length);

                    return int.TryParse(sequence, out int value)
                        ? value
                        : 0;
                })
                .DefaultIfEmpty(0)
                .Max() + 1;

            string referenceNumber =
                $"{currentYear}_{nextDcrNumber:D3}";

            // 15. Create DCR request.
            var dcrRequest = new DCRRequest
            {
                ReferenceNumber = referenceNumber,

                RequestDate = request.RequestDate,

                DepartmentId = departmentId,

                Standard = request.Standard.Trim(),

                RequestType = requestType,

                DocumentTypeId = documentType!.DocumentTypeId,

                DocumentId = existingDocument?.DocumentId,

                DocumentTitle = requestType == "Addition"
                    ? request.DocumentTitle!.Trim()
                    : existingDocument?.Title,

                DocumentNo = documentNo,

                CurrentRevisionNumber = currentRevisionNumber,

                NewRevisionNumber = newRevisionNumber,

                ProcessOwnerId = departmentHead.Id,

                DescriptionBeforechange =
                    request.DescriptionBeforechange,

                DescriptionAfterchange =
                    request.DescriptionAfterchange,

                Reasonforchange =
                    request.Reasonforchange,

                CreatedByUserId = currentUser.Id,

                Status = "Submitted",

                CreatedAt = indiaTime
            };

            _context.DCRRequests.Add(dcrRequest);

            await _context.SaveChangesAsync();

            // 16. Create workflow instance.
            var workflowInstance = new WorkFlowInstance
            {
                DCRRequestId = dcrRequest.Id,

                DepartmentApprovalMappingId = mapping.Id,

                Status = "InProgress",

                StartedAt = indiaTime
            };

            _context.WorkFlowInstances.Add(workflowInstance);

            await _context.SaveChangesAsync();

            // 17. Create workflow stages.
            foreach (var level in approvalLevels)
            {
                var workflowStage = new WorkflowStage
                {
                    WorkflowInstanceId = workflowInstance.Id,

                    StageOrder = level.LevelNumber,

                    DepartmentApprovalLevelId = level.Id,

                    ApproverId = approverLookup[level.ApproverEmployeeId],

                    Status = "Pending"
                };

                _context.WorkflowStages.Add(workflowStage);
            }

            await _context.SaveChangesAsync();

            // 18. Commit the transaction.
            await transaction.CommitAsync();

            return referenceNumber;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
