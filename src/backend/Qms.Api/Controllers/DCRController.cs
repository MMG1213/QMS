
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qms.Api.DTOs;
using Qms.Api.Services;

namespace Qms.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DCRController : ControllerBase
{
    private readonly IDCRService _dcrService;

    public DCRController(IDCRService dcrService)
    {
        _dcrService = dcrService;
    }

    // POST: api/DCR
    [HttpPost]
    public async Task<IActionResult> CreateDCR(
        [FromBody] DCRCreateDto request)
    {
        try
        {
            var referenceNumber =
                await _dcrService.CreateDCRAsync(request);

            return Ok(new
            {
                success = true,
                message = "DCR submitted successfully.",
                referenceNumber = referenceNumber
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }
}
