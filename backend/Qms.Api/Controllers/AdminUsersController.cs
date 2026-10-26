
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qms.Api.Data;
using Qms.Api.DTOs;
using Qms.Api.Models;

namespace Qms.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = QmsRole.QmsAdmin)]
public class AdminUsersController : ControllerBase
{
    private readonly QmsDbContext _context;
    private readonly IPasswordHasher<AppUser> _passwordHasher;

    public AdminUsersController(
        QmsDbContext context,
        IPasswordHasher<AppUser> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    // GET: api/admin/users
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.AppUsers
            .AsNoTracking()
            .OrderBy(u => u.FullName)
            .Select(u => new
            {
                u.Id,
                u.EmployeeId,
                u.FullName,
                u.Email,
                u.DepartmentId,
                DepartmentName = u.Department == null
                    ? null
                    : u.Department.Name,
                u.Role,
                u.IsActive,
                u.CreatedAtUtc
            })
            .ToListAsync();

        return Ok(users);
    }

    // GET: api/admin/users/departments
    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments()
    {
        var departments = await _context.Departments
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

    // POST: api/admin/users
    [HttpPost]
    public async Task<IActionResult> CreateUser(
        [FromBody] AdminCreateUserRequest request)
    {
        var employeeId = request.EmployeeId.Trim();
        var fullName = request.FullName.Trim();
        var email = request.Email.Trim();

        if (string.IsNullOrWhiteSpace(employeeId) ||
            string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                message = "All required fields must be provided."
            });
        }

        var allowedRoles = new[]
        {
            QmsRole.User,
            QmsRole.DepartmentHead
        };

        if (!allowedRoles.Contains(request.Role, StringComparer.Ordinal))
        {
            return BadRequest(new { message = "Invalid role." });
        }

        // Only the global QMS Admin may have no department.
        if (request.Role == QmsRole.QmsAdmin)
        {
            if (request.DepartmentId.HasValue)
            {
                return BadRequest(new
                {
                    message = "A QMS Admin must not be assigned a department."
                });
            }
        }
        else
        {
            if (!request.DepartmentId.HasValue)
            {
                return BadRequest(new
                {
                    message = "A department is required for this role."
                });
            }

            var departmentExists = await _context.Departments
                .AnyAsync(d =>
                    d.Id == request.DepartmentId.Value &&
                    d.IsActive);

            if (!departmentExists)
            {
                return BadRequest(new
                {
                    message = "The selected department is invalid or inactive."
                });
            }
        }

        var employeeExists = await _context.AppUsers
            .AnyAsync(u => u.EmployeeId == employeeId);

        if (employeeExists)
        {
            return Conflict(new
            {
                message = "Employee ID is already registered."
            });
        }

        var emailExists = await _context.AppUsers
            .AnyAsync(u => u.Email == email);

        if (emailExists)
        {
            return Conflict(new
            {
                message = "Email is already registered."
            });
        }

        var user = new AppUser
        {
            EmployeeId = employeeId,
            FullName = fullName,
            Email = email,
            DepartmentId = request.DepartmentId,
            Role = request.Role,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password);

        _context.AppUsers.Add(user);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Handles a duplicate insert that races with another request.
            return Conflict(new
            {
                message = "Employee ID or email is already registered."
            });
        }

        return Created(
            $"/api/admin/users/{user.Id}",
            new
            {
                user.Id,
                user.EmployeeId,
                user.FullName,
                user.Email,
                user.DepartmentId,
                user.Role,
                user.IsActive
            });
    }

    // PATCH: api/admin/users/{id}/status
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] AdminUpdateUserStatusRequest request)
    {
        var user = await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new { message = "Account not found." });
        }

        var currentAdminId = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (user.Role == QmsRole.QmsAdmin &&
            user.IsActive &&
            !request.IsActive &&
            currentAdminId == user.Id.ToString())
        {
            return BadRequest(new
            {
                message = "You cannot deactivate your own account."
            });
        }

        if (user.Role == QmsRole.QmsAdmin &&
            user.IsActive &&
            !request.IsActive)
        {
            var otherActiveAdmins = await _context.AppUsers
                .CountAsync(u =>
                    u.Role == QmsRole.QmsAdmin &&
                    u.IsActive &&
                    u.Id != user.Id);

            if (otherActiveAdmins == 0)
            {
                return BadRequest(new
                {
                    message = "The last active QMS Admin cannot be deactivated."
                });
            }
        }

        user.IsActive = request.IsActive;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = request.IsActive
                ? "Account activated."
                : "Account deactivated.",
            user.Id,
            user.IsActive
        });
    }
}
