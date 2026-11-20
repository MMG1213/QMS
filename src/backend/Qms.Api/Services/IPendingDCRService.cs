using Qms.Api.DTOs;

namespace Qms.Api.Services;

public interface IPendingDCRService
{
    Task<List<DCRPendingDto>> GetPendingDCRsAsync();
}