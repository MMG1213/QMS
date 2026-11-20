using Qms.Api.DTOs;

namespace Qms.Api.Services;

public interface IDCRService
{
    Task<string> CreateDCRAsync(DCRCreateDto request);
}