
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
        var departments = await _db.Departments
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.DeptName)
            .Select(d => new
            {
                d.Id,
                d.DeptName
            })
            .ToListAsync();

        return Ok(departments);
    }
}
