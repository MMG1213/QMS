
using Qms.Api.DTOs;

namespace Qms.Api.Services;

public interface ISigninService
{
    Task<LoginServiceResult> LoginAsync(LoginRequest request);
}

public class LoginServiceResult
{
    public bool Succeeded { get; private set; }
    public bool IsConfigurationError { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public LoginResponse? Response { get; private set; }

    public static LoginServiceResult Success(LoginResponse response)
    {
        return new LoginServiceResult
        {
            Succeeded = true,
            Response = response
        };
    }

    public static LoginServiceResult Failure(string message)
    {
        return new LoginServiceResult
        {
            Message = message
        };
    }

    public static LoginServiceResult ConfigurationError(string message)
    {
        return new LoginServiceResult
        {
            IsConfigurationError = true,
            Message = message
        };
    }
}
