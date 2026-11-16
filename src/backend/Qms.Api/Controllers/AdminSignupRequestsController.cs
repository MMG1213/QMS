
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qms.Api.DTOs;
using Qms.Api.Models;
using Qms.Api.Services;

namespace Qms.Api.Controllers;

[ApiController]
[Route("api/admin/signup-requests")]
[Authorize(Roles = QmsRole.QmsAdmin)]
public class AdminSignupRequestsController : ControllerBase
{
    private readonly IAdminSignupRequestService _adminSignupService;

    public AdminSignupRequestsController(
        IAdminSignupRequestService adminSignupService)
    {
        _adminSignupService = adminSignupService;
    }

    // GET: api/admin/signup-requests?status=Pending
    [HttpGet]
    public async Task<IActionResult> GetRequests(
        [FromQuery] string status = AccountRequestStatus.Pending)
    {
        var allowedStatuses = new[]
        {
            AccountRequestStatus.Pending,
            AccountRequestStatus.Approved,
            AccountRequestStatus.Rejected
        };

        if (string.IsNullOrWhiteSpace(status) ||
            !allowedStatuses.Contains(
                status,
                StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Invalid request status."
            });
        }

        var normalizedStatus = allowedStatuses.First(s =>
            s.Equals(status, StringComparison.OrdinalIgnoreCase));

        var requests =
            await _adminSignupService.GetRequestsAsync(
                normalizedStatus);

        return Ok(requests);
    }

    // POST: api/admin/signup-requests/{id}/review
    [HttpPost("{id:int}/review")]
    public async Task<IActionResult> Review(
        int id,
        [FromBody] ReviewSignupRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Decision))
        {
            return BadRequest(new
            {
                message = "Decision must be Approve or Reject."
            });
        }

        var reviewerIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(reviewerIdValue, out var reviewerId))
        {
            return Unauthorized();
        }

        var result = await _adminSignupService.ReviewAsync(
            id,
            request.Decision,
            reviewerId);

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
            requestId = result.RequestId,
            status = result.Status
        });
    }
}
