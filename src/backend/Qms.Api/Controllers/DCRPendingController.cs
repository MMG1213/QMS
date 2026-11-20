using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qms.Api.Services;

namespace Qms.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DCRPendingController : ControllerBase
{
    private readonly IPendingDCRService _pendingDCRService;

    public DCRPendingController(IPendingDCRService pendingDCRService)
    {
        _pendingDCRService = pendingDCRService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPendingDCRs()
    {
        try
        {
            var pendingDCRs =
                await _pendingDCRService.GetPendingDCRsAsync();

            return Ok(new
            {
                success = true,
                count = pendingDCRs.Count,
                data = pendingDCRs
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
    }
}