
using Microsoft.EntityFrameworkCore;
using Qms.Api.Data;
using Qms.Api.DTOs;
using Qms.Api.Models;

namespace Qms.Api.Services;

public class DocumentApprovalMatrixService
    : IDocumentApprovalMatrixService
{
    private readonly QmsDbContext _db;

    public DocumentApprovalMatrixService(QmsDbContext db)
    {
        _db = db;
    }

    public async Task<DocumentApprovalMatrixDto?> GetByDocumentTypeAsync(
        int documentTypeId,
        CancellationToken cancellationToken = default)
    {
        var matrix = await _db.DocumentApprovalMatrices
            .AsNoTracking()
            .Where(x => x.DocumentTypeId == documentTypeId)
            .Select(x => new DocumentApprovalMatrixDto
            {
                DocumentTypeId = x.DocumentTypeId,
                ApprovedBy = x.ApprovedBy,
                CheckedBy = x.CheckedBy,
                IssuedBy = x.IssuedBy,
                ApprovedDate = x.ApprovedDate,
                CheckedDate = x.CheckedDate,
                IssuedDate = x.IssuedDate
            })
            .SingleOrDefaultAsync(cancellationToken);

        return matrix;
    }

    public async Task<DocumentApprovalMatrixDto> SaveAsync(
        SaveDocApprovalMatrixRequest request,
        CancellationToken cancellationToken = default)
    {
        var documentTypeExists = await _db.DocumentTypes
            .AnyAsync(
                x => x.DocumentTypeId == request.DocumentTypeId,
                cancellationToken);

        if (!documentTypeExists)
        {
            throw new KeyNotFoundException(
                $"Document Type ID {request.DocumentTypeId} was not found.");
        }

        var matrix = await _db.DocumentApprovalMatrices
            .SingleOrDefaultAsync(
                x => x.DocumentTypeId == request.DocumentTypeId,
                cancellationToken);

        if (matrix == null)
        {
            matrix = new DocumentApprovalMatrix
            {
                DocumentTypeId = request.DocumentTypeId
            };

            _db.DocumentApprovalMatrices.Add(matrix);
        }

        matrix.ApprovedBy = request.ApprovedBy;
        matrix.CheckedBy = request.CheckedBy;
        matrix.IssuedBy = request.IssuedBy;

        matrix.ApprovedDate = request.ApprovedDate;
        matrix.CheckedDate = request.CheckedDate;
        matrix.IssuedDate = request.IssuedDate;

        await _db.SaveChangesAsync(cancellationToken);

        return new DocumentApprovalMatrixDto
        {
            DocumentTypeId = matrix.DocumentTypeId,
            ApprovedBy = matrix.ApprovedBy,
            CheckedBy = matrix.CheckedBy,
            IssuedBy = matrix.IssuedBy,
            ApprovedDate = matrix.ApprovedDate,
            CheckedDate = matrix.CheckedDate,
            IssuedDate = matrix.IssuedDate
        };
    }
}
