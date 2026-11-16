using Microsoft.AspNetCore.Mvc;
using Qms.Api.Services;

namespace Qms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ML01controller : ControllerBase
{
    private readonly ML01Service _service;
    public ML01controller(ML01Service service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var entries = await _service.GetML01Entries(cancellationToken);

        return Ok(entries);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ML01EntryRequest request, CancellationToken cancellationToken)
    {
        var entry = await _service.CreateAsync(request, cancellationToken);

        return StatusCode(201, entry);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ML01EntryRequest request, CancellationToken cancellationToken)
    {
        var entry = await _service.UpdateAsync(id, request, cancellationToken);

        if (entry == null)
        {
            return NotFound();
        }

        return Ok(entry);
    }
}