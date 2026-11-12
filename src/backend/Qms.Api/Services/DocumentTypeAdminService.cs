
using Microsoft.EntityFrameworkCore;
using Qms.Api.Data;
using Qms.Api.DTOs;
using Qms.Api.Models;

namespace Qms.Api.Services;

public class DocumentTypeAdminService : IDocumentTypeAdminService
{
    private readonly QmsDbContext _db;

    public DocumentTypeAdminService(QmsDbContext db)
    {
        _db = db;
    }

    public async Task<List<DocumentTypeAdminDto>>
        GetAllDocumentTypesAsync()
    {
        return await _db.DocumentTypes
            .AsNoTracking()
            .OrderBy(t => t.TypeName)
            .Select(t => new DocumentTypeAdminDto
            {
                DocumentTypeId = t.DocumentTypeId,
                TypeCode = t.TypeCode,
                TypeName = t.TypeName,
                IsActive = t.IsActive
            })
            .ToListAsync();
    }

    public async Task<DocumentTypeAdminDto?>
        GetDocumentTypeByIdAsync(int id)
    {
        return await _db.DocumentTypes
            .AsNoTracking()
            .Where(t => t.DocumentTypeId == id)
            .Select(t => new DocumentTypeAdminDto
            {
                DocumentTypeId = t.DocumentTypeId,
                TypeCode = t.TypeCode,
                TypeName = t.TypeName,
                IsActive = t.IsActive
            })
            .SingleOrDefaultAsync();
    }

    public async Task<DocumentTypeOperationResult>
        CreateDocumentTypeAsync(CreateDocumentTypeRequest request)
    {
        var typeCode = request.TypeCode?.Trim() ?? string.Empty;
        var typeName = request.TypeName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(typeCode) ||
            string.IsNullOrWhiteSpace(typeName))
        {
            return DocumentTypeOperationResult.Failure(
                400,
                "Document type code and name are required.");
        }

        if (typeCode.Length > 20 || typeName.Length > 100)
        {
            return DocumentTypeOperationResult.Failure(
                400,
                "Document type code must be at most 20 characters and name at most 100 characters.");
        }

        var duplicateExists = await _db.DocumentTypes.AnyAsync(t =>
            t.TypeCode == typeCode || t.TypeName == typeName);

        if (duplicateExists)
        {
            return DocumentTypeOperationResult.Failure(
                409,
                "A document type with that code or name already exists.");
        }

        var documentType = new DocumentType
        {
            TypeCode = typeCode,
            TypeName = typeName,
            IsActive = true
        };

        _db.DocumentTypes.Add(documentType);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return DocumentTypeOperationResult.Failure(
                409,
                "The document type could not be created because its code may already exist.");
        }

        var response = new DocumentTypeAdminDto
        {
            DocumentTypeId = documentType.DocumentTypeId,
            TypeCode = documentType.TypeCode,
            TypeName = documentType.TypeName,
            IsActive = documentType.IsActive
        };

        return DocumentTypeOperationResult.Success(
            response,
            "Document type created successfully.",
            201);
    }

    public async Task<DocumentTypeOperationResult>
        SetDocumentTypeStatusAsync(int id, bool isActive)
    {
        var documentType = await _db.DocumentTypes
            .SingleOrDefaultAsync(t => t.DocumentTypeId == id);

        if (documentType == null)
        {
            return DocumentTypeOperationResult.Failure(
                404,
                "Document type not found.");
        }

        documentType.IsActive = isActive;

        await _db.SaveChangesAsync();

        var response = new DocumentTypeAdminDto
        {
            DocumentTypeId = documentType.DocumentTypeId,
            TypeCode = documentType.TypeCode,
            TypeName = documentType.TypeName,
            IsActive = documentType.IsActive
        };

        var message = isActive
            ? "Document type activated successfully."
            : "Document type deactivated successfully.";

        return DocumentTypeOperationResult.Success(
            response,
            message);
    }
}
