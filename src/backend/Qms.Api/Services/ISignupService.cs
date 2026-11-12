
using Qms.Api.DTOs;

namespace Qms.Api.Services;

public interface ISignupService
{
    Task<SignupServiceResult> SubmitAsync(SignupRequestDto request);
}

public class SignupServiceResult
{
    public bool Succeeded { get; private set; }
    public int StatusCode { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public int? RequestId { get; private set; }
    public string? Status { get; private set; }

    public static SignupServiceResult Success(
        int requestId,
        string status,
        string message)
    {
        return new SignupServiceResult
        {
            Succeeded = true,
            StatusCode = 200,
            RequestId = requestId,
            Status = status,
            Message = message
        };
    }

    public static SignupServiceResult Failure(
        int statusCode,
        string message)
    {
        return new SignupServiceResult
        {
            Succeeded = false,
            StatusCode = statusCode,
            Message = message
        };
    }
}
