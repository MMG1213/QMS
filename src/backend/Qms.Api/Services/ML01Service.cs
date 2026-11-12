
using Microsoft.EntityFrameworkCore;
using Qms.Api.Data;
using Qms.Api.Models;
using Qms.Api.Dtos;

namespace Qms.Api.Services;

public class ML01Service
{
    private readonly QmsDbContext _context;

    public ML01Service(QmsDbContext context)
    {
        _context = context;
    }

    // GET: All ML01 entries
    public async Task<List<ML01EntryDto>> GetML01Entries(
        CancellationToken cancellationToken)
    {
        return await _context.ML01Entries
            .AsNoTracking()
            .Where(e => e.DocumentType.TypeCode == "ML01")
            .OrderBy(e => e.Id)
            .Select(e => new ML01EntryDto
            {
                Id = e.Id,
                Description = e.Description,
                DocumentNo = e.DocumentNo,

                Revisions = e.Revisions
                    .OrderBy(r => r.RevisionNo)
                    .Select(r => new ML01RevisionDto
                    {
                       Id = r.Id,
                        RevisionNo = r.RevisionNo,
                        PageCount = r.PageCount,
                        RevisionDate = r.RevisionDate
                    }).ToList()
            })
            .ToListAsync(cancellationToken);
    }

    // CREATE: New ML01 entry
    public async Task<ML01EntryDto> CreateAsync(
        ML01EntryRequest request,
        CancellationToken cancellationToken)
    {
        var documentTypeId = await GetML01DocumentTypeIdAsync(
            cancellationToken);

        var entry = new ML01Entry
        {
            Description = request.Description ?? string.Empty,
            DocumentNo = request.DocumentNo ?? string.Empty,
            DocumentTypeId = documentTypeId,

            Revisions = request.Revisions.Select(r => new ML01Revision
            {
                RevisionNo = r.RevisionNo,
                PageCount = r.PageCount,
                RevisionDate = r.RevisionDate
            }).ToList()
        };

        _context.ML01Entries.Add(entry);

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(entry);
    }

    // UPDATE: Existing ML01 entry
    public async Task<ML01EntryDto?> UpdateAsync(
        int id,
        ML01EntryRequest request,
        CancellationToken cancellationToken)
    {
        var entry = await _context.ML01Entries
            .Include(e => e.Revisions)
            .Where(e => e.Id == id &&
                        e.DocumentType.TypeCode == "ML01")
            .FirstOrDefaultAsync(cancellationToken);

        if (entry == null)
        {
            return null;
        }

        entry.Description = request.Description ?? string.Empty;
        entry.DocumentNo = request.DocumentNo ?? string.Empty;

        // Remove existing revisions
        _context.ML01Revisions.RemoveRange(entry.Revisions);

        // Add updated revisions
        entry.Revisions = request.Revisions.Select(r => new ML01Revision
        {
            RevisionNo = r.RevisionNo,
            PageCount = r.PageCount,
            RevisionDate = r.RevisionDate
        }).ToList();

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(entry);
    }

    // Get ML01 DocumentTypeId
    private async Task<int> GetML01DocumentTypeIdAsync(
        CancellationToken cancellationToken)
    {
        var documentTypeId = await _context.DocumentTypes
            .Where(t => t.TypeCode == "ML01")
            .Select(t => (int?)t.DocumentTypeId)
            .FirstOrDefaultAsync(cancellationToken);

        return documentTypeId
            ?? throw new InvalidOperationException(
                "ML01 document type was not found.");
    }

    // Convert Entity to Response DTO
    private static ML01EntryDto MapToDto(ML01Entry entry)
    {
        return new ML01EntryDto
        {
           Id = entry.Id,
            Description = entry.Description,
            DocumentNo = entry.DocumentNo,

            Revisions = entry.Revisions
                .OrderBy(r => r.RevisionNo)
                .Select(r => new ML01RevisionDto
                {
                   Id = r.Id,
                    RevisionNo = r.RevisionNo,
                    PageCount = r.PageCount,
                    RevisionDate = r.RevisionDate
                }).ToList()
        };
    }
}
