
using Microsoft.AspNetCore.Mvc;
using Qms.Api.Services;

namespace Qms.Api.Controllers;

[ApiController]
[Route("api/master-list-import")]
public class MasterListImportController : ControllerBase
{
    private readonly MasterListExcelPreviewService _previewService;

    public MasterListImportController(
        MasterListExcelPreviewService previewService)
    {
        _previewService = previewService;
    }

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
            var result = _previewService.Preview(filePath);
            return Ok(result);
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
}
