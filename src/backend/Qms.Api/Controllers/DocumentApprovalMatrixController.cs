
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qms.Api.DTOs;
using Qms.Api.Models;
using Qms.Api.Services;

namespace Qms.Api.Controllers;

[ApiController]
[Route("api/document-approval-matrix")]
[Authorize(Roles = QmsRole.User + "," +
                   QmsRole.DepartmentHead + "," +
                   QmsRole.QmsAdmin)]
public class DocumentApprovalMatrixController : ControllerBase
{
    private readonly IDocumentApprovalMatrixService _service;

    public DocumentApprovalMatrixController(
        IDocumentApprovalMatrixService service)
    {
        _service = service;
    }

    // GET: api/document-approval-matrix/{documentTypeId}
    // Accessible to User, Department Head, and QMS Admin.
    [HttpGet("{documentTypeId:int}")]
    public async Task<IActionResult> GetByDocumentType(
        int documentTypeId,
        CancellationToken cancellationToken)
    {
        if (documentTypeId <= 0)
        {
            return BadRequest(new
            {
                message = "Document Type ID must be greater than 0."
            });
        }

        var matrix = await _service.GetByDocumentTypeAsync(
            documentTypeId,
            cancellationToken);

        if (matrix == null)
        {
            return Ok(new DocumentApprovalMatrixDto
            {
                DocumentTypeId = documentTypeId
            });
        }

        return Ok(matrix);
    }

    // PUT: api/document-approval-matrix
    // QMS Admin only.
    [HttpPut]
    [Authorize(Roles = QmsRole.QmsAdmin)]
    public async Task<IActionResult> Save(
        [FromBody] SaveDocApprovalMatrixRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var matrix = await _service.SaveAsync(
                request,
                cancellationToken);

            return Ok(new
            {
                message = "Document Approval Matrix saved successfully.",
                data = matrix
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }
}
