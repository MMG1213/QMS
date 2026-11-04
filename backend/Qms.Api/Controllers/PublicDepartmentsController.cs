
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qms.Api.Data;

namespace Qms.Api.Controllers;

[ApiController]
[Route("api/departments")]
public class PublicDepartmentsController : ControllerBase
{
    private readonly QmsDbContext _db;

    public PublicDepartmentsController(QmsDbContext db)
    {
        _db = db;
    }

    [AllowAnonymous]
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveDepartments()
    {
        var departments = await _db.QMSDepartments
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .Select(d => new
            {
                id = d.DepartmentId,
                deptName = d.DepartmentName
            })
            .ToListAsync();

        return Ok(departments);
    }
}
