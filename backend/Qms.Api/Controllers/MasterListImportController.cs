
using Microsoft.AspNetCore.Mvc;
using Qms.Api.Services;

namespace Qms.Api.Controllers;

[ApiController]
[Route("api/master-list-import")]
public class MasterListImportController : ControllerBase
{
    private readonly MasterListExcelPreviewService _previewService;
    private readonly IMasterListImportService _importService;

    public MasterListImportController(
        MasterListExcelPreviewService previewService,
        IMasterListImportService importService)
    {
        _previewService = previewService;
        _importService = importService;
    }

    // Existing preview endpoint.
    [HttpGet("preview")]
    public IActionResult Preview()
    {
        string filePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "ImportFiles",
            "MasterList.xlsx");

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound(new
            {
                message = "Excel file not found.",
                expectedPath = filePath
            });
        }

        try
        {
            return Ok(_previewService.Preview(filePath));
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = "Could not preview the Excel workbook.",
                details = ex.Message
            });
        }
    }

    // Imports the four master lists through the service.
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> Import(
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken)
    {
        var result = await _importService.ImportAsync(
            file,
            cancellationToken);

        return result.Status switch
        {
            MasterListImportStatus.Success =>
                Ok(result.Body),

            MasterListImportStatus.InvalidInput =>
                BadRequest(result.Body),

            MasterListImportStatus.Conflict =>
                Conflict(result.Body),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "An unexpected error occurred." })
        };
    }
}
