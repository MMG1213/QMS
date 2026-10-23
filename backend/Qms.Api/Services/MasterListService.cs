using Qms.Api.Models;
using Qms.Api.Data;
using Qms.Api.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using System.Transactions;
using System.Reflection;
using System.Linq;
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

    public async Task<List<MasterListDocumentDto>> GetDocumentsAsync(int? documentTypeId, int? departmentId,String? Search)
    {
        var query = _context.Documents.AsNoTracking().AsQueryable();

        if (documentTypeId.HasValue)
        {
            query = query.Where(d=>d.DocumentTypeId == documentTypeId.Value);
        }

        if (departmentId.HasValue)
        {
            query = query.Where(d=>d.DepartmentId == departmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(Search))
        {
            query = query.Where(d => d.DocumentNo.Contains(Search) || d.Title.Contains(Search));
        }

        return await query.OrderBy(d => d.DocumentNo)
        .Select(document => new MasterListDocumentDto
        {
            Title = document.Title,
            DocumentNo = document.DocumentNo,
            DepartmentName = _context.QMSDepartments
                .Where(department =>
                    department.DepartmentId == document.DepartmentId)
                .Select(department => department.DepartmentName)
                .FirstOrDefault(),
            RetentionPeriod =document.RetentionPeriod,
            Status=document.Status,

            Revisions = document.Revisions.OrderBy(d => d.RevisionNumber).Select(
                d => new DocumentRevisionDto
                {
                    RevisionNumber = d.RevisionNumber,
                    PageCount = d.PageCount,
                    RevisionDate = d.RevisionDate,
                }
            ).ToList()
        }).ToListAsync();
    }
}
