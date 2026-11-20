
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qms.Api.DTOs.DepartmentApprovalMappings;
using Qms.Api.Services.Interfaces;
using System.Security.Claims;

namespace Qms.Api.Controllers;

[ApiController]
[Route("api/department-approval-mappings")]
[Authorize]
public class DepartmentApprovalMappingsController : ControllerBase
{
    private readonly IDepartmentApprovalMappingService _service;

    public DepartmentApprovalMappingsController(
        IDepartmentApprovalMappingService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound(new { message = "Mapping not found." });

        return Ok(result);
    }

    [HttpGet("employees/{departmentId:int}")]
    public async Task<IActionResult> GetEmployees(int departmentId)
    {
        try
        {
            var result = await _service.GetEmployeesAsync(departmentId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "QMS Admin")]
    public async Task<IActionResult> Create(
        [FromBody] CreateDepartmentApprovalMappingDto dto)
    {
        var employeeId =
            User.FindFirst("employeeId")?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrWhiteSpace(employeeId))
            return Unauthorized(new { message = "Employee ID claim missing." });

        var result = await _service.CreateAsync(dto, employeeId);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.MappingId },
            new
            {
                message = result.Message,
                mappingId = result.MappingId,
                configVersion = result.ConfigVersion
            });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "QMS Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateDepartmentApprovalMappingDto dto)
    {
        var employeeId =
            User.FindFirst("employeeId")?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrWhiteSpace(employeeId))
            return Unauthorized(new { message = "Employee ID claim missing." });

        var result = await _service.UpdateAsync(
            id,
            dto,
            employeeId);

        if (!result.Success)
            return BadRequest(new { message = result.Message });

        return Ok(new { message = result.Message });
    }
}
