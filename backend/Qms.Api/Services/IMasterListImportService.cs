
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace Qms.Api.Services;

public interface IMasterListImportService
{
    Task<MasterListImportResult> ImportAsync(
        IFormFile? file,
        CancellationToken cancellationToken);
}

public enum MasterListImportStatus
{
    Success,
    InvalidInput,
    Conflict
}

public sealed class MasterListImportResult
{
    public MasterListImportStatus Status { get; init; }

    public object Body { get; init; } = new { };

    public static MasterListImportResult Success(object body)
    {
        return new MasterListImportResult
        {
            Status = MasterListImportStatus.Success,
            Body = body
        };
    }

    public static MasterListImportResult InvalidInput(object body)
    {
        return new MasterListImportResult
        {
            Status = MasterListImportStatus.InvalidInput,
            Body = body
        };
    }

    public static MasterListImportResult Conflict(object body)
    {
        return new MasterListImportResult
        {
            Status = MasterListImportStatus.Conflict,
            Body = body
        };
    }
}
