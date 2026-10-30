
using Microsoft.AspNetCore.Mvc;
using Qms.Api.DTOs;
using Qms.Api.Services;

namespace Qms.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class SigninController : ControllerBase
{
    private readonly ISigninService _SigninService;

    public SigninController(ISigninService SigninService)
    {
        _SigninService = SigninService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request)
    {
        var result = await _SigninService.LoginAsync(request);

        if (result.IsConfigurationError)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Authentication configuration error",
                detail: result.Message);
        }

        if (!result.Succeeded)
        {
            return Unauthorized(new
            {
                message = result.Message
            });
        }

        return Ok(result.Response);
    }
}
