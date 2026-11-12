
using Qms.Api.DTOs;

namespace Qms.Api.Services;

public interface IDocumentTypeAdminService
{
    Task<List<DocumentTypeAdminDto>> GetAllDocumentTypesAsync();

    Task<DocumentTypeAdminDto?> GetDocumentTypeByIdAsync(int id);

    Task<DocumentTypeOperationResult> CreateDocumentTypeAsync(
        CreateDocumentTypeRequest request);

    Task<DocumentTypeOperationResult> SetDocumentTypeStatusAsync(
        int id,
        bool isActive);
}

public class DocumentTypeOperationResult
{
    public bool Succeeded { get; private set; }

    public int StatusCode { get; private set; }

    public string Message { get; private set; } = string.Empty;

    public DocumentTypeAdminDto? DocumentType { get; private set; }

    public static DocumentTypeOperationResult Success(
        DocumentTypeAdminDto documentType,
        string message,
        int statusCode = 200)
    {
        return new DocumentTypeOperationResult
        {
            Succeeded = true,
            StatusCode = statusCode,
            Message = message,
            DocumentType = documentType
        };
    }

    public static DocumentTypeOperationResult Failure(
        int statusCode,
        string message)
    {
        return new DocumentTypeOperationResult
        {
            Succeeded = false,
            StatusCode = statusCode,
            Message = message
        };
    }
}
