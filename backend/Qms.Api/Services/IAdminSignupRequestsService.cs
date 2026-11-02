
using Qms.Api.DTOs;

namespace Qms.Api.Services;

public interface IAdminSignupRequestService
{
    Task<List<AdminSignupRequestDto>> GetRequestsAsync(
        string status);

    Task<AdminSignupReviewResult> ReviewAsync(
        int id,
        string decision,
        int reviewerId);
}

public class AdminSignupReviewResult
{
    public bool Succeeded { get; private set; }
    public int StatusCode { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public int RequestId { get; private set; }
    public string Status { get; private set; } = string.Empty;

    public static AdminSignupReviewResult Success(
        int requestId,
        string status,
        string message)
    {
        return new AdminSignupReviewResult
        {
            Succeeded = true,
            StatusCode = 200,
            RequestId = requestId,
            Status = status,
            Message = message
        };
    }

    public static AdminSignupReviewResult Failure(
        int statusCode,
        string message)
    {
        return new AdminSignupReviewResult
        {
            Succeeded = false,
            StatusCode = statusCode,
            Message = message
        };
    }
}
