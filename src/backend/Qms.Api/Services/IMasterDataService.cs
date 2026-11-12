
using Qms.Api.DTOs;

namespace Qms.Api.Services;

public interface IMasterDataService
{
    Task<List<MachineListDto>> GetMachinesAsync(
        CancellationToken cancellationToken = default);

    Task<MachineListDto> CreateMachineAsync(
        MachineListDto request,
        CancellationToken cancellationToken = default);

    Task<MachineListDto?> UpdateMachineAsync(
        int id,
        MachineListDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteMachineAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<List<StandardDto>> GetStandardsAsync(
        CancellationToken cancellationToken = default);

    Task<StandardDto> CreateStandardAsync(
        StandardDto request,
        CancellationToken cancellationToken = default);

    Task<StandardDto?> UpdateStandardAsync(
        int id,
        StandardDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteStandardAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<List<TestingOrMeasuringInstrumentDto>>
        GetInstrumentsAsync(
            CancellationToken cancellationToken = default);

    Task<TestingOrMeasuringInstrumentDto> CreateInstrumentAsync(
        TestingOrMeasuringInstrumentDto request,
        CancellationToken cancellationToken = default);

    Task<TestingOrMeasuringInstrumentDto?> UpdateInstrumentAsync(
        int id,
        TestingOrMeasuringInstrumentDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteInstrumentAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<List<QualityPerformanceIndexDto>> GetQpiAsync(
        CancellationToken cancellationToken = default);

    Task<QualityPerformanceIndexDto> CreateQpiAsync(
        QualityPerformanceIndexDto request,
        CancellationToken cancellationToken = default);

    Task<QualityPerformanceIndexDto?> UpdateQpiAsync(
        int id,
        QualityPerformanceIndexDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteQpiAsync(
        int id,
        CancellationToken cancellationToken = default);
}
