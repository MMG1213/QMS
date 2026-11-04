using Microsoft.AspNetCore.Mvc;
using Qms.Api.Services;
using Qms.Api.Dtos;

namespace Qms.Api.Controllers;

[ApiController]
[Route("api/master-list")]
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

    [HttpGet("document-types")]
    public async Task<IActionResult> GetDocumentTypes()
    {
        var documentTypes = await _service.GetDocumentTypesAsync();
        return Ok(documentTypes);
    }

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments()
    {
        var departments = await _service.GetQMSDepartmentsAsync();
        return Ok(departments);
    }

    [HttpGet("documents")]
    public async Task<IActionResult> GetDocuments(
        [FromQuery] int? documentTypeId,
        [FromQuery] int? departmentId,
        [FromQuery] string? search)
    {
        var documents = await _service.GetDocumentsAsync(
            documentTypeId,
            departmentId,
            search);

        return Ok(documents);
    }

    [HttpPut("documents/{documentId:int}")]
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
                    message = "Document not found"
                });
            }

            return Ok(new
            {
                message = "Master List Document Updated Successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Excel export
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

            var fileName = $"MasterList_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}