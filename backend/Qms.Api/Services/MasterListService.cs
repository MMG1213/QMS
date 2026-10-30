using Qms.Api.Models;
using Qms.Api.Data;
using Qms.Api.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Transactions;
using System.Reflection;
using System.Linq;
using System.Data.Common;
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

        return await _context.DocumentTypes.AsNoTracking().OrderBy(type => type.TypeName).Select(type => new DocumentTypeDto
        {
            DocumentTypeId = type.DocumentTypeId,
            TypeName = type.TypeName,
        }).ToListAsync();
    }

    public async Task<List<QMSDepartmentDto>> GetQMSDepartmentsAsync()
    {
        return await _context.QMSDepartments.AsNoTracking().OrderBy(d => d.DepartmentName).Select(d => new QMSDepartmentDto
        {
            DepartmentId = d.DepartmentId,
            DepartmentCode = d.DepartmentCode,
            DepartmentName = d.DepartmentName,
        }).ToListAsync();
    }

    public async Task<List<MasterListDocumentDto>> GetDocumentsAsync(int? documentTypeId, int? departmentId, String? Search)
    {
        var query = _context.Documents.AsNoTracking().AsQueryable();

        if (documentTypeId.HasValue)
        {
            query = query.Where(d => d.DocumentTypeId == documentTypeId.Value);
        }

        if (departmentId.HasValue)
        {
            query = query.Where(d => d.DepartmentId == departmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(Search))
        {
            query = query.Where(d => d.DocumentNo.Contains(Search) || d.Title.Contains(Search));
        }

        return await query.OrderBy(d => d.DocumentNo)
        .Select(document => new MasterListDocumentDto
        {
            DocumentId = document.DocumentId,
            Title = document.Title,
            DocumentNo = document.DocumentNo,
            DepartmentName = _context.QMSDepartments
                .Where(department =>
                    department.DepartmentId == document.DepartmentId)
                .Select(department => department.DepartmentName)
                .FirstOrDefault(),
            TypeName=_context.DocumentTypes.Where(documenttype=>documenttype.DocumentTypeId==document.DocumentTypeId)
            .Select(documenttype=>documenttype.TypeName).FirstOrDefault(),
            RetentionPeriod = document.RetentionPeriod,
            Status = document.Status,
            ObseleteDate = document.ObseleteDate,

            Revisions = document.Revisions.OrderBy(d => d.RevisionNumber).Select(
                d => new DocumentRevisionDto
                {
                    Id = d.Id,
                    RevisionNumber = d.RevisionNumber,
                    PageCount = d.PageCount,
                    RevisionDate = d.RevisionDate,
                }
            ).ToList()
        }).ToListAsync();
    }

    public async Task<bool> UpdateDocumentAsync(int documentid, UpdateMasterListDto request)
    {
        var document = await _context.Documents.Include(d => d.Revisions).FirstOrDefaultAsync(d => d.DocumentId==documentid);

        if (document == null) return false;

        request.DocumentNo = request.DocumentNo.Trim();
        request.Title = request.Title.Trim();

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title Is Required.");

        if (string.IsNullOrWhiteSpace(request.DocumentNo))
            throw new ArgumentException("Document Number Is Required.");

        var duplicateDocumentNo = await _context.Documents.AnyAsync(d => d.DocumentId != documentid && d.DocumentNo == request.DocumentNo);
        if (duplicateDocumentNo)
            throw new ArgumentException("The Document Number Is Already In Use!");

        if (!await _context.Documents.AnyAsync(t => t.DocumentTypeId == request.DocumentTypeId))
            throw new ArgumentException("Invalid Document Type.");

        if (!await _context.Documents.AnyAsync(t => t.DepartmentId == request.DepartmentId))
            throw new ArgumentException("Invalid Department.");

        if (request.Status == "Obsolete" && request.ObseleteDate == null)
            throw new ArgumentException("An obsolete date is required for an obsolete document.");

        if (request.Status == "Active" && request.ObseleteDate != null)
            throw new ArgumentException("An Active Document cannot have an Obselete Date");

        var existingNumbers = document.Revisions.Select(r => r.RevisionNumber).OrderBy(n => n).ToList();

        var submittedNumber = request.Revisions.Select(r => r.RevisionNumber).OrderBy(n => n).ToList();
        
        if (submittedNumber.Count != submittedNumber
        .Distinct().Count() || !existingNumbers.SequenceEqual(submittedNumber))
            throw new ArgumentException("Revision numbers cannot be added, deleted, or changed here.");

        foreach (var revisionDto in request.Revisions)
        {
            if (revisionDto.PageCount < 1)
            {
                throw new ArgumentException("Page Count Must be Greater than Zero.");
            }

            if (revisionDto.RevisionDate == default)
                throw new ArgumentException("Every Revision must have a  Valid Date");
        }

        document.DocumentNo = request.DocumentNo;
        document.Title = request.Title;
        document.DepartmentId = request.DepartmentId;
        document.DocumentTypeId = request.DocumentTypeId;
        document.RetentionPeriod = string.IsNullOrWhiteSpace(request.RetentionPeriod) ? null : request.RetentionPeriod.Trim();
        document.Status = request.Status;
        document.ObseleteDate = request.ObseleteDate;

        foreach (var revisionDto in request.Revisions)
        {
            var revision = document.Revisions.First(r => r.RevisionNumber == revisionDto.RevisionNumber);

            revision.PageCount = revisionDto.PageCount;
            revision.RevisionDate = revisionDto.RevisionDate.Date;
        }
        await _context.SaveChangesAsync();
        return true;
    }
}
