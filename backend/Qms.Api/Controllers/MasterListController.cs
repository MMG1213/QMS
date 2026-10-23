using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using Microsoft.AspNetCore.Mvc;
using Qms.Api.Services;
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
}

