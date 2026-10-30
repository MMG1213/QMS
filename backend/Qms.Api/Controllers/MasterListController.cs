using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using Microsoft.AspNetCore.Mvc;
using Qms.Api.Services;
using Qms.Api.Dtos;
namespace Qms.Api.Controllers;

[ApiController]
[Route("api/master-list")]

public class MasterListController : ControllerBase
{
    private readonly MasterListService _service;

    public MasterListController(MasterListService service)
    {
        _service = service;
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
        [FromQuery] String? Search
    )
    {
        var document = await _service.GetDocumentsAsync(documentTypeId, departmentId, Search);

        return Ok(document);
    }

    [HttpPut("documents/{documentId:int}")]

    public async Task<IActionResult> UpdateDocuments(int documentId, [FromBody] UpdateMasterListDto request)
    {
        try
        {
            var updated = await _service.UpdateDocumentAsync(documentId, request);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Document not found"
                });
            }

            return Ok(new
            {
                message = "Master List Document Updated Sucessfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

