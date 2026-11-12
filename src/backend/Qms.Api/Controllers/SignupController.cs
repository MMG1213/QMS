
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qms.Api.DTOs;
using Qms.Api.Services;

namespace Qms.Api.Controllers;

[ApiController]
[Route("api/signup")]
public class SignupController : ControllerBase
{
    private readonly ISignupService _signupService;

    public SignupController(ISignupService signupService)
    {
        _signupService = signupService;
    }

    // POST: api/signup
    // Public endpoint: saves a pending request, never an active account.
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Submit(
        [FromBody] SignupRequestDto request)
    {
        var result = await _signupService.SubmitAsync(request);

        if (result.Succeeded)
        {
            return Ok(new
            {
                message = result.Message,
                requestId = result.RequestId,
                status = result.Status
            });
        }

        return StatusCode(result.StatusCode, new
        {
            message = result.Message
        });
    }
}
