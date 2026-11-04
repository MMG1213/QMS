
using Qms.Api.DTOs;

namespace Qms.Api.Services;

public interface IDocumentApprovalMatrixService
{
    Task<DocumentApprovalMatrixDto?> GetByDocumentTypeAsync(
        int documentTypeId,
        CancellationToken cancellationToken = default);

    Task<DocumentApprovalMatrixDto> SaveAsync(
        SaveDocApprovalMatrixRequest request,
        CancellationToken cancellationToken = default);
}
