
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qms.Api.Models;
using Qms.Api.Services;
using Qms.Api.DTOs;

namespace Qms.Api.Controllers;

[ApiController]
[Route("api/master-list")]
[Authorize]
public class MasterListController : ControllerBase
{
    private readonly MasterListService _service;
    private readonly MasterListExcelExportService _excelExportService;

    public MasterListController(
        MasterListService service,
        MasterListExcelExportService excelExportService)
    {
        _service = service;
        _excelExportService = excelExportService;
    }

    // GET: api/master-list/document-types
    [HttpGet("document-types")]
    public async Task<IActionResult> GetDocumentTypes()
    {
        var documentTypes = await _service.GetDocumentTypesAsync();

        return Ok(documentTypes);
    }

    // GET: api/master-list/departments
    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments()
    {
        var departments = await _service.GetQMSDepartmentsAsync();

        return Ok(departments);
    }

    // GET: api/master-list/documents
    // Optional filters:
    // documentTypeId, departmentId, search, status, standard
    [HttpGet("documents")]
    public async Task<IActionResult> GetDocuments(
        [FromQuery] int? documentTypeId,
        [FromQuery] int? departmentId,
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? standard,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1)
        {
            return BadRequest(new
            {
                message = "Page number must be at least 1."
            });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                message = "Page size must be between 1 and 100."
            });
        }

        try
        {
            var result = await _service.GetDocumentsAsync(
                documentTypeId,
                departmentId,
                search,
                status,
                standard,
                pageNumber,
                pageSize);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // POST: api/master-list/documents
    // Add a new document for ML02-ML06 and ML11.
    [HttpPost("documents")]
    [Authorize(Roles = QmsRole.QmsAdmin)]
    public async Task<IActionResult> AddDocument(
        [FromBody] AddDocumentDto request)
    {
        try
        {
            var documentId = await _service.AddDocumentAsync(request);

            return CreatedAtAction(
                nameof(GetDocuments),
                new
                {
                    documentTypeId = request.DocumentTypeId
                },
                new
                {
                    documentId,
                    message = "Document added successfully."
                });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // PUT: api/master-list/documents/{documentId}
    // Only QMS Admin can update Master List documents.
    [HttpPut("documents/{documentId:int}")]
    [Authorize(Roles = QmsRole.QmsAdmin)]
    public async Task<IActionResult> UpdateDocuments(
        int documentId,
        [FromBody] UpdateMasterListDto request)
    {
        try
        {
            var updated = await _service.UpdateDocumentAsync(
                documentId,
                request);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Document not found."
                });
            }

            return Ok(new
            {
                message = "Master List Document Updated Successfully."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // GET: api/master-list/export
    [HttpGet("export")]
    public async Task<IActionResult> ExportMasterList(
        [FromQuery] int? documentTypeId,
        [FromQuery] int? departmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var fileBytes = await _excelExportService.GenerateExcelAsync(
                documentTypeId,
                departmentId,
                cancellationToken);

            var fileName =
                $"MasterList_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // PUT: api/master-list/obsolete
    [HttpPut("obsolete")]
    public async Task<IActionResult> BulkobsoleteDocumets(
        [FromBody] BulkObsoleteDocuments request)
    {
        try
        {
            var count = await _service.BulkobsoleteDocumentAsync(request);

            return Ok(new
            {
                message = $"{count} documents marked as obsolete successfully.",
                updatedCount = count
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}
