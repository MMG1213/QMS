
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Qms.Api.Data;
using Qms.Api.DTOs;
using Qms.Api.Models;

namespace Qms.Api.Controllers;

[ApiController]
[Route("api/admin/signup-requests")]
[Authorize(Roles = QmsRole.QmsAdmin)]
public class AdminSignupRequestsController : ControllerBase
{
    private readonly QmsDbContext _db;
    private readonly IPasswordHasher<AppUser> _passwordHasher;

    public AdminSignupRequestsController(
        QmsDbContext db,
        IPasswordHasher<AppUser> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    // GET: api/admin/signup-requests?status=Pending
    [HttpGet]
    public async Task<IActionResult> GetRequests(
        [FromQuery] string status = AccountRequestStatus.Pending)
    {
        var allowedStatuses = new[]
        {
            AccountRequestStatus.Pending,
            AccountRequestStatus.Approved,
            AccountRequestStatus.Rejected
        };

        if (string.IsNullOrWhiteSpace(status) ||
            !allowedStatuses.Contains(
                status,
                StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Invalid request status." });
        }

        var normalizedStatus = allowedStatuses.First(s =>
            s.Equals(status, StringComparison.OrdinalIgnoreCase));

        // Explicitly select safe fields. Password hashes and setup tokens
        // are never returned to the frontend.
        var requests = await _db.AccountRequests
            .AsNoTracking()
            .Where(r => r.Status == normalizedStatus)
            .OrderBy(r => r.RequestedAtUtc)
            .Select(r => new
            {
                r.Id,
                r.EmployeeId,
                r.FullName,
                r.Email,
                r.DepartmentId,
                DepartmentName = r.Department == null
                    ? null
                    : r.Department.DeptName,
                r.Role,
                r.Status,
                r.RequestedAtUtc,
                r.ReviewedByUserId,
                r.ReviewedAtUtc
            })
            .ToListAsync();

        return Ok(requests);
    }

    // POST: api/admin/signup-requests/{id}/review
    [HttpPost("{id:int}/review")]
    public async Task<IActionResult> Review(
        int id,
        [FromBody] ReviewSignupRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Decision))
        {
            return BadRequest(new
            {
                message = "Decision must be Approve or Reject."
            });
        }

        var decision = request.Decision.Trim();

        var approved = decision.Equals(
            "Approve",
            StringComparison.OrdinalIgnoreCase);

        var rejected = decision.Equals(
            "Reject",
            StringComparison.OrdinalIgnoreCase);

        if (!approved && !rejected)
        {
            return BadRequest(new
            {
                message = "Decision must be Approve or Reject."
            });
        }

        var reviewerIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(reviewerIdValue, out var reviewerId))
        {
            return Unauthorized();
        }

        var reviewerIsActiveAdmin = await _db.AppUsers.AnyAsync(u =>
            u.Id == reviewerId &&
            u.IsActive &&
            u.Role == QmsRole.QmsAdmin);

        if (!reviewerIsActiveAdmin)
        {
            return Forbid();
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        var accountRequest = await _db.AccountRequests
            .FirstOrDefaultAsync(r => r.Id == id);

        if (accountRequest == null)
        {
            return NotFound(new
            {
                message = "Signup request not found."
            });
        }

        if (accountRequest.Status != AccountRequestStatus.Pending)
        {
            return Conflict(new
            {
                message = "Only pending signup requests can be reviewed."
            });
        }

        if (approved)
        {
            if (string.IsNullOrWhiteSpace(accountRequest.PasswordHash))
            {
                return Conflict(new
                {
                    message = "This request has no saved password hash. Ask the employee to submit a new signup request."
                });
            }

            if (accountRequest.Role != QmsRole.User &&
                accountRequest.Role != QmsRole.DepartmentHead)
            {
                return Conflict(new
                {
                    message = "The requested role is invalid."
                });
            }

            var departmentExists = await _db.Departments.AnyAsync(d =>
                d.Id == accountRequest.DepartmentId && d.IsActive);

            if (!departmentExists)
            {
                return Conflict(new
                {
                    message = "The requested department is no longer active."
                });
            }

            var accountAlreadyExists = await _db.AppUsers.AnyAsync(u =>
                u.EmployeeId == accountRequest.EmployeeId ||
                u.Email == accountRequest.Email);

            if (accountAlreadyExists)
            {
                return Conflict(new
                {
                    message = "An account already exists for this employee ID or email."
                });
            }

            var newUser = new AppUser
            {
                EmployeeId = accountRequest.EmployeeId,
                FullName = accountRequest.FullName,
                Email = accountRequest.Email,
                DepartmentId = accountRequest.DepartmentId,
                Role = accountRequest.Role,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            // Transfer the already-hashed password to the new account.
            // The original password is never retrieved or returned.
            newUser.PasswordHash = accountRequest.PasswordHash;

            _db.AppUsers.Add(newUser);

            accountRequest.Status = AccountRequestStatus.Approved;
            accountRequest.PasswordHash = null;
        }
        else
        {
            accountRequest.Status = AccountRequestStatus.Rejected;
            accountRequest.PasswordHash = null;
        }

        // Retire any legacy password-setup token data.
        accountRequest.PasswordSetupTokenHash = null;
        accountRequest.PasswordSetupTokenExpiresAtUtc = null;
        accountRequest.ReviewedByUserId = reviewerId;
        accountRequest.ReviewedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(new
        {
            message = approved
                ? "Signup request approved. The employee can now sign in with the password submitted during signup."
                : "Signup request rejected.",
            requestId = accountRequest.Id,
            status = accountRequest.Status
        });
    }
}
