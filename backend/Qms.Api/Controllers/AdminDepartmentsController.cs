
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qms.Api.DTOs;
using Qms.Api.Models;
using Qms.Api.Services;

namespace Qms.Api.Controllers;

[ApiController]
[Route("api/admin/departments")]
[Authorize(Roles = QmsRole.QmsAdmin)]
public class AdminDepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public AdminDepartmentsController(
        IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    // GET: api/admin/departments
    // Returns all departments, including inactive ones.
    [HttpGet]
    public async Task<IActionResult> GetAllDepartments()
    {
        var departments =
            await _departmentService.GetAllDepartmentsAsync();

        return Ok(departments);
    }

    // GET: api/admin/departments/{id}
    // Returns the details and status of one department.
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDepartmentById(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "A valid department ID is required."
            });
        }

        var department =
            await _departmentService.GetDepartmentByIdAsync(id);

        if (department == null)
        {
            return NotFound(new
            {
                message = "Department not found."
            });
        }

        return Ok(department);
    }

    // POST: api/admin/departments
    // Creates a new active department.
    [HttpPost]
    public async Task<IActionResult> CreateDepartment(
        [FromBody] CreateDepartmentRequest request)
    {
        var result =
            await _departmentService.CreateDepartmentAsync(request);

        if (!result.Succeeded)
        {
            return StatusCode(result.StatusCode, new
            {
                message = result.Message
            });
        }

        return Created(
            $"/api/admin/departments/{result.Department!.Id}",
            new
            {
                message = result.Message,
                department = result.Department
            });
    }

    // PATCH: api/admin/departments/{id}/status
    // Activates or deactivates a department.
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetDepartmentStatus(
        int id,
        [FromBody] UpdateDepartmentStatusRequest request)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "A valid department ID is required."
            });
        }

        var result = await _departmentService.SetDepartmentStatusAsync(
            id,
            request.IsActive);

        if (!result.Succeeded)
        {
            return StatusCode(result.StatusCode, new
            {
                message = result.Message
            });
        }

        return Ok(new
        {
            message = result.Message,
            department = result.Department
        });
    }
}
