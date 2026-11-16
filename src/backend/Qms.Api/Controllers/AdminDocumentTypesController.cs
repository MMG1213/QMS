
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qms.Api.DTOs;
using Qms.Api.Models;
using Qms.Api.Services;

namespace Qms.Api.Controllers;

[ApiController]
[Route("api/admin/document-types")]
[Authorize(Roles = QmsRole.QmsAdmin)]
public class AdminDocumentTypesController : ControllerBase
{
    private readonly IDocumentTypeAdminService _documentTypeService;

    public AdminDocumentTypesController(
        IDocumentTypeAdminService documentTypeService)
    {
        _documentTypeService = documentTypeService;
    }

    // GET: api/admin/document-types
    // Returns all document types, including inactive ones.
    [HttpGet]
    public async Task<IActionResult> GetAllDocumentTypes()
    {
        var documentTypes =
            await _documentTypeService.GetAllDocumentTypesAsync();

        return Ok(documentTypes);
    }

    // GET: api/admin/document-types/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDocumentTypeById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "A valid document type ID is required."
            });
        }

        var documentType =
            await _documentTypeService.GetDocumentTypeByIdAsync(id);

        if (documentType == null)
        {
            return NotFound(new
            {
                message = "Document type not found."
            });
        }

        return Ok(documentType);
    }

    // POST: api/admin/document-types
    // Creates a new active document type.
    [HttpPost]
    public async Task<IActionResult> CreateDocumentType(
        [FromBody] CreateDocumentTypeRequest request)
    {
        var result =
            await _documentTypeService.CreateDocumentTypeAsync(request);

        if (!result.Succeeded)
        {
            return StatusCode(result.StatusCode, new
            {
                message = result.Message
            });
        }

        return Created(
            $"/api/admin/document-types/{result.DocumentType!.DocumentTypeId}",
            new
            {
                message = result.Message,
                documentType = result.DocumentType
            });
    }

    // PATCH: api/admin/document-types/{id}/status
    // Activates or deactivates a document type.
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetDocumentTypeStatus(
        int id,
        [FromBody] UpdateDocumentTypeStatusRequest request)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "A valid document type ID is required."
            });
        }

        var result =
            await _documentTypeService.SetDocumentTypeStatusAsync(
                id,
                request.IsActive);

        if (!result.Succeeded)
        {
            return StatusCode(result.StatusCode, new
            {
                message = result.Message
            });
        }

        return Ok(new
        {
            message = result.Message,
            documentType = result.DocumentType
        });
    }
}
