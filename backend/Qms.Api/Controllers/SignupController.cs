
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qms.Api.Data;
using Qms.Api.DTOs;
using Qms.Api.Models;

namespace Qms.Api.Controllers;

[ApiController]
[Route("api/signup")]
public class SignupController : ControllerBase
{
    private readonly QmsDbContext _db;
    private readonly IPasswordHasher<AppUser> _passwordHasher;

    public SignupController(
        QmsDbContext db,
        IPasswordHasher<AppUser> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    // POST: api/signup
    // Public endpoint: saves a pending request, never an active account.
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Submit(
        [FromBody] SignupRequestDto request)
    {
        var employeeId = request.EmployeeId.Trim();
        var fullName = request.FullName.Trim();
        var email = request.Email.Trim();
        var role = request.Role.Trim();

        if (string.IsNullOrWhiteSpace(employeeId) ||
            string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(new
            {
                message = "Employee ID, full name, and email are required."
            });
        }

        if (role != QmsRole.User &&
            role != QmsRole.DepartmentHead)
        {
            return BadRequest(new
            {
                message = "Requested role must be User or Department Head."
            });
        }

        var departmentExists = await _db.Departments.AnyAsync(d =>
            d.Id == request.DepartmentId && d.IsActive);

        if (!departmentExists)
        {
            return BadRequest(new
            {
                message = "Select a valid active department."
            });
        }

        var accountExists = await _db.AppUsers.AnyAsync(u =>
            u.EmployeeId == employeeId || u.Email == email);

        if (accountExists)
        {
            return Conflict(new
            {
                message = "An account with that employee ID or email already exists."
            });
        }

        var pendingRequestExists = await _db.AccountRequests.AnyAsync(r =>
            r.Status == AccountRequestStatus.Pending &&
            (r.EmployeeId == employeeId || r.Email == email));

        if (pendingRequestExists)
        {
            return Conflict(new
            {
                message = "A pending signup request already exists for that employee ID or email."
            });
        }

        // Hash the password immediately. Never save the plaintext password.
        var passwordHash = _passwordHasher.HashPassword(
            new AppUser(),
            request.Password);

        var accountRequest = new AccountRequest
        {
            EmployeeId = employeeId,
            FullName = fullName,
            Email = email,
            DepartmentId = request.DepartmentId,
            Role = role,
            PasswordHash = passwordHash,
            Status = AccountRequestStatus.Pending,
            RequestedAtUtc = DateTime.UtcNow
        };

        _db.AccountRequests.Add(accountRequest);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new
            {
                message = "The signup request could not be saved because of a data conflict."
            });
        }

        return Ok(new
        {
            message = "Signup request submitted for QMS Admin review.",
            requestId = accountRequest.Id,
            status = accountRequest.Status
        });
    }
}
