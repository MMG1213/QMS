
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qms.Api.DTOs;
using Qms.Api.Services;

namespace Qms.Api.Controllers;
[ApiController]
[Route("api/master-data")]
[Authorize]
public class MasterDataController : ControllerBase
{
  private readonly IMasterDataService _service;

  public MasterDataController(IMasterDataService service)
  {
    _service = service;
  }

  [HttpGet("machines")]
  public async Task<IActionResult> GetMachines(
      CancellationToken cancellationToken)
  {
    var result = await _service.GetMachinesAsync(cancellationToken);
    return Ok(result);
  }

    [HttpPost("machines")]
    [Authorize(Roles = "QMS Admin")]
    public async Task<IActionResult> CreateMachine(
        [FromBody] MachineListDto request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateMachineAsync(
            request, cancellationToken);

        return CreatedAtAction(
            nameof(GetMachines),
            new { },
            result);
    }

    [HttpPut("machines/{id:int}")]
    [Authorize(Roles = "QMS Admin")]
    public async Task<IActionResult> UpdateMachine(
        int id,
        [FromBody] MachineListDto request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateMachineAsync(
            id, request, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("machines/{id:int}")]
    [Authorize(Roles = "QMS Admin")]
    public async Task<IActionResult> DeleteMachine(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteMachineAsync(
            id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    // ───────────── Standards ─────────────

    [HttpGet("standards")]
    public async Task<IActionResult> GetStandards(
        CancellationToken cancellationToken)
    {
        var result = await _service.GetStandardsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("standards")]
    [Authorize(Roles = "QMS Admin")]
    public async Task<IActionResult> CreateStandard(
        [FromBody] StandardDto request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateStandardAsync(
            request, cancellationToken);

        return CreatedAtAction(nameof(GetStandards), result);
    }

    [HttpPut("standards/{id:int}")]
    [Authorize(Roles = "QMS Admin")]
    public async Task<IActionResult> UpdateStandard(
        int id,
        [FromBody] StandardDto request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateStandardAsync(
            id, request, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("standards/{id:int}")]
    [Authorize(Roles = "QMS Admin")]
    public async Task<IActionResult> DeleteStandard(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteStandardAsync(
            id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    // ───── Testing and Measuring Instruments ─────

    [HttpGet("instruments")]
    public async Task<IActionResult> GetInstruments(
        CancellationToken cancellationToken)
    {
        var result = await _service.GetInstrumentsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("instruments")]
    [Authorize(Roles = "QMS Admin")]
    public async Task<IActionResult> CreateInstrument(
        [FromBody] TestingOrMeasuringInstrumentDto request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateInstrumentAsync(
            request, cancellationToken);

        return CreatedAtAction(nameof(GetInstruments), result);
    }

    [HttpPut("instruments/{id:int}")]
    [Authorize(Roles = "QMS Admin")]
    public async Task<IActionResult> UpdateInstrument(
        int id,
        [FromBody] TestingOrMeasuringInstrumentDto request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateInstrumentAsync(
            id, request, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("instruments/{id:int}")]
    [Authorize(Roles = "QMS Admin")]
    public async Task<IActionResult> DeleteInstrument(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteInstrumentAsync(
            id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    // ───────────── Quality Performance Index ─────────────

    [HttpGet("qpi")]
    public async Task<IActionResult> GetQpi(
        CancellationToken cancellationToken)
    {
        var result = await _service.GetQpiAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("qpi")]
    [Authorize(Roles = "QMS Admin")]
    public async Task<IActionResult> CreateQpi(
        [FromBody] QualityPerformanceIndexDto request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateQpiAsync(
            request, cancellationToken);

        return CreatedAtAction(nameof(GetQpi), result);
    }

    [HttpPut("qpi/{id:int}")]
    [Authorize(Roles = "QMS Admin")]
    public async Task<IActionResult> UpdateQpi(
        int id,
        [FromBody] QualityPerformanceIndexDto request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateQpiAsync(
            id, request, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("qpi/{id:int}")]
    [Authorize(Roles = "QMS Admin")]
    public async Task<IActionResult> DeleteQpi(
        int id,
        CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteQpiAsync(
            id, cancellationToken);

        return deleted ? NoContent() : NotFound();
    }
}
