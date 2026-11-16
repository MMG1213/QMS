
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qms.Api.Services;

namespace Qms.Api.Controllers;

[ApiController]
[Route("api/departments")]
public class PublicDepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public PublicDepartmentsController(
        IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [AllowAnonymous]
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveDepartments()
    {
        var departments = await _departmentService.GetActiveDepartmentsAsync();
        var response = departments.Select(d => new
        {
            id = d.DepartmentId,
            deptName = d.DepartmentName
        });

        return Ok(response);
    }
}
