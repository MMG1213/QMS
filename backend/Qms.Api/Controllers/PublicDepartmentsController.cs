
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

    // GET: api/departments/active
    // Returns only the information needed for the public signup form.
    [AllowAnonymous]
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveDepartments()
    {
        var departments = await _db.Departments
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new
            {
                d.Id,
                d.Name
            })
            .ToListAsync();

        return Ok(departments);
    }
}
