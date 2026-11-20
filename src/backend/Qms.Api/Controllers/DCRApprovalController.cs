
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qms.Api.DTOs;
using Qms.Api.Services;

namespace Qms.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DCRApprovalController : ControllerBase
{
    private readonly IWorkflowApprovalService _approvalService;

    public DCRApprovalController(
        IWorkflowApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    // POST: api/DCRApproval/1
    [HttpPost("{dcrRequestId:int}")]
    public async Task<IActionResult> ApproveOrReject(
        int dcrRequestId,
        [FromBody] DCRApprovalDto request)
    {
        try
        {
            await _approvalService.ApproveOrRejectAsync(
                dcrRequestId,
                request);

            return Ok(new
            {
                success = true,
                message = "DCR approval action recorded successfully."
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                success = false,
                message = ex.Message
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
