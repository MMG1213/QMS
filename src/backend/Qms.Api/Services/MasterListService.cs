
using Qms.Api.Models;
using Qms.Api.Data;
using Qms.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Qms.Api.Services;

public class MasterListService
{
    private readonly QmsDbContext _context;

    public MasterListService(QmsDbContext context)
    {
        _context = context;
    }

    public async Task<List<DocumentTypeDto>> GetDocumentTypesAsync()
    {
        return await _context.DocumentTypes
            .AsNoTracking()
            .OrderBy(type => type.TypeName)
            .Select(type => new DocumentTypeDto
            {
                DocumentTypeId = type.DocumentTypeId,
                TypeCode = type.TypeCode,
                TypeName = type.TypeName,
                IsActive = type.IsActive
            })
            .ToListAsync();
    }

    public async Task<List<QMSDepartmentDto>> GetQMSDepartmentsAsync()
    {
        return await _context.QMSDepartments
            .AsNoTracking()
            .OrderBy(d => d.DepartmentName)
            .Select(d => new QMSDepartmentDto
            {
                DepartmentId = d.DepartmentId,
                DepartmentCode = d.DepartmentCode,
                DepartmentName = d.DepartmentName
            })
            .ToListAsync();
    }

    // Get documents with optional Standard filtering.
    public async Task<PagedResultDto<MasterListDocumentDto>> GetDocumentsAsync(
        int? documentTypeId,
        int? departmentId,
        string? search,
        string? status,
        string? standard,
        int pageNumber,
        int pageSize)
    {
        var query = _context.Documents
            .AsNoTracking()
            .AsQueryable();

        if (documentTypeId.HasValue)
        {
            query = query.Where(d =>
                d.DocumentTypeId == documentTypeId.Value);
        }

        if (departmentId.HasValue)
        {
            query = query.Where(d =>
                d.DepartmentId == departmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(standard))
        {
            var standardValue = standard.Trim();

            query = query.Where(d =>
                d.Standard == standardValue);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();

            query = query.Where(d =>
                d.DocumentNo.Contains(searchTerm) ||
                d.Title.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();

        var activeCount = await query.CountAsync(d =>
            d.Status == "Active");

        var obsoleteCount = await query.CountAsync(d =>
            d.Status == "Obsolete");

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim();

            if (!normalizedStatus.Equals(
                    "Active",
                    StringComparison.OrdinalIgnoreCase) &&
                !normalizedStatus.Equals(
                    "Obsolete",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Status must be Active or Obsolete.");
            }

            query = query.Where(d =>
                d.Status == normalizedStatus);
        }

        var filteredCount = await query.CountAsync();

        var totalPages = filteredCount == 0
            ? 0
            : (int)Math.Ceiling(
                filteredCount / (double)pageSize);

        if (totalPages > 0 && pageNumber > totalPages)
        {
            pageNumber = totalPages;
        }

        var items = await query
            .OrderBy(d => d.DocumentNo)
            .ThenBy(d => d.DocumentId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(document => new MasterListDocumentDto
            {
                DocumentId = document.DocumentId,
                Title = document.Title,
                DocumentNo = document.DocumentNo,

                DepartmentName = _context.QMSDepartments
                    .Where(department =>
                        department.DepartmentId ==
                        document.DepartmentId)
                    .Select(department =>
                        department.DepartmentName)
                    .FirstOrDefault(),

                TypeName = _context.DocumentTypes
                    .Where(documentType =>
                        documentType.DocumentTypeId ==
                        document.DocumentTypeId)
                    .Select(documentType =>
                        documentType.TypeName)
                    .FirstOrDefault() ?? string.Empty,

                Standard = document.Standard,
                RetentionPeriod = document.RetentionPeriod,
                Status = document.Status,
                ObseleteDate = document.ObseleteDate,

                Revisions = document.Revisions
                    .OrderBy(revision =>
                        revision.RevisionNumber)
                    .Select(revision => new DocumentRevisionDto
                    {
                        Id = revision.Id,
                        RevisionNumber = revision.RevisionNumber,
                        PageCount = revision.PageCount,
                        RevisionDate = revision.RevisionDate
                    })
                    .ToList()
            })
            .ToListAsync();

        return new PagedResultDto<MasterListDocumentDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = filteredCount,
            AllCount = totalCount,
            TotalPages = totalPages,
            ActiveCount = activeCount,
            ObsoleteCount = obsoleteCount
        };
    }

    // Add a new controlled document.
    public async Task<int> AddDocumentAsync(AddDocumentDto request)
    {
        // Only ML02-ML06 and ML11 can use this operation.
        int[] allowedDocumentTypeIds = { 2, 3, 4, 5, 6, 11 };

        if (!allowedDocumentTypeIds.Contains(request.DocumentTypeId))
        {
            throw new ArgumentException(
                "Documents can only be added for ML02-ML06 and ML11.");
        }

        request.DocumentNo =
            (request.DocumentNo ?? string.Empty).Trim();

        request.Title =
            (request.Title ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(request.DocumentNo))
        {
            throw new ArgumentException(
                "Document Number is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException(
                "Document Title is required.");
        }

        if (await _context.Documents.AnyAsync(d =>
                d.DocumentNo == request.DocumentNo))
        {
            throw new ArgumentException(
                "The Document Number is already in use.");
        }

        var documentType = await _context.DocumentTypes
            .FirstOrDefaultAsync(t =>
                t.DocumentTypeId == request.DocumentTypeId &&
                t.IsActive);

        if (documentType == null)
        {
            throw new ArgumentException(
                "Invalid or inactive Document Type.");
        }

        var departmentExists = await _context.QMSDepartments
            .AnyAsync(d =>
                d.DepartmentId == request.DepartmentId);

        if (!departmentExists)
        {
            throw new ArgumentException(
                "Invalid Department.");
        }

        if (request.PageCount < 1)
        {
            throw new ArgumentException(
                "Page Count must be greater than zero.");
        }

        if (request.RevisionDate == default)
        {
            throw new ArgumentException(
                "A valid Revision Date is required.");
        }

        var standard = string.IsNullOrWhiteSpace(request.Standard)
            ? null
            : request.Standard.Trim();

        var document = new Document
        {
            DocumentNo = request.DocumentNo,
            Title = request.Title,
            DocumentTypeId = request.DocumentTypeId,
            DepartmentId = request.DepartmentId,
            Standard = standard,

            RetentionPeriod =
                string.IsNullOrWhiteSpace(request.RetentionPeriod)
                    ? null
                    : request.RetentionPeriod.Trim(),

            Status = "Active",
            ObseleteDate = null,

            Revisions = new List<DocumentRevision>
            {
                new DocumentRevision
                {
                    RevisionNumber = 0,
                    PageCount = request.PageCount,
                    RevisionDate = request.RevisionDate.Date
                }
            }
        };

        _context.Documents.Add(document);

        await _context.SaveChangesAsync();

        return document.DocumentId;
    }

    // Update an existing Master List document.
    public async Task<bool> UpdateDocumentAsync(
        int documentid,
        UpdateMasterListDto request)
    {
        var document = await _context.Documents
            .Include(d => d.Revisions)
            .FirstOrDefaultAsync(d =>
                d.DocumentId == documentid);

        if (document == null)
        {
            return false;
        }

        request.DocumentNo =
            (request.DocumentNo ?? string.Empty).Trim();

        request.Title =
            (request.Title ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Title Is Required.");
        }

        if (string.IsNullOrWhiteSpace(request.DocumentNo))
        {
            throw new ArgumentException(
                "Document Number Is Required.");
        }

        var duplicateDocumentNo = await _context.Documents
            .AnyAsync(d =>
                d.DocumentId != documentid &&
                d.DocumentNo == request.DocumentNo);

        if (duplicateDocumentNo)
        {
            throw new ArgumentException(
                "The Document Number Is Already In Use!");
        }

        if (!await _context.DocumentTypes.AnyAsync(t =>
                t.DocumentTypeId == request.DocumentTypeId))
        {
            throw new ArgumentException(
                "Invalid Document Type.");
        }

        if (!await _context.QMSDepartments.AnyAsync(d =>
                d.DepartmentId == request.DepartmentId))
        {
            throw new ArgumentException(
                "Invalid Department.");
        }

        request.Standard = string.IsNullOrWhiteSpace(request.Standard)
            ? null
            : request.Standard.Trim();

        if (request.Status == "Obsolete" &&
            request.ObseleteDate == null)
        {
            throw new ArgumentException(
                "An obsolete date is required for an obsolete document.");
        }

        if (request.Status == "Active" &&
            request.ObseleteDate != null)
        {
            throw new ArgumentException(
                "An Active Document cannot have an Obselete Date.");
        }

        var existingNumbers = document.Revisions
            .Select(r => r.RevisionNumber)
            .OrderBy(n => n)
            .ToList();

        var submittedNumber = request.Revisions
            .Select(r => r.RevisionNumber)
            .OrderBy(n => n)
            .ToList();

        if (submittedNumber.Count != submittedNumber.Distinct().Count() ||
            !existingNumbers.SequenceEqual(submittedNumber))
        {
            throw new ArgumentException(
                "Revision numbers cannot be added, deleted, or changed here.");
        }

        foreach (var revisionDto in request.Revisions)
        {
            if (revisionDto.PageCount < 1)
            {
                throw new ArgumentException(
                    "Page Count Must be Greater than Zero.");
            }

            if (revisionDto.RevisionDate == default)
            {
                throw new ArgumentException(
                    "Every Revision must have a Valid Date.");
            }
        }

        document.DocumentNo = request.DocumentNo;
        document.Title = request.Title;
        document.DepartmentId = request.DepartmentId;
        document.DocumentTypeId = request.DocumentTypeId;
        document.Standard = request.Standard;

        document.RetentionPeriod =
            string.IsNullOrWhiteSpace(request.RetentionPeriod)
                ? null
                : request.RetentionPeriod.Trim();

        document.Status = request.Status;
        document.ObseleteDate = request.ObseleteDate;

        foreach (var revisionDto in request.Revisions)
        {
            var revision = document.Revisions.First(
                r => r.RevisionNumber ==
                     revisionDto.RevisionNumber);

            revision.PageCount = revisionDto.PageCount;
            revision.RevisionDate =
                revisionDto.RevisionDate.Date;
        }

        await _context.SaveChangesAsync();

        return true;
    }

    // Mark multiple documents as obsolete.
    public async Task<int> BulkobsoleteDocumentAsync(
        BulkObsoleteDocuments request)
    {
        if (request.DocumentIds == null ||
            request.DocumentIds.Count == 0)
        {
            throw new ArgumentException(
                "Please select Atleast one Document.");
        }

        var selectdocuments = request.DocumentIds
            .Distinct()
            .ToList();

        if (selectdocuments.Any(id => id <= 0))
        {
            throw new ArgumentException(
                "Invalid document Ids.");
        }

        var documents = await _context.Documents
            .Where(d =>
                selectdocuments.Contains(d.DocumentId) &&
                d.Status == "Active")
            .ToListAsync();

        if (documents.Count != selectdocuments.Count)
        {
            throw new ArgumentException(
                "Some selected documents does not exist or are already obsolete.");
        }

        var obsoleteDate =
            DateOnly.FromDateTime(DateTime.Today);

        foreach (var document in documents)
        {
            document.Status = "Obsolete";
            document.ObseleteDate = obsoleteDate;
        }

        await _context.SaveChangesAsync();

        return documents.Count;
    }
}
