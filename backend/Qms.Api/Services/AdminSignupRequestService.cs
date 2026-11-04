
using Microsoft.EntityFrameworkCore;
using Qms.Api.Data;
using Qms.Api.DTOs;
using Qms.Api.Models;

namespace Qms.Api.Services;

public class AdminSignupRequestService : IAdminSignupRequestService
{
    private readonly QmsDbContext _db;

    public AdminSignupRequestService(QmsDbContext db)
    {
        _db = db;
    }

    public async Task<List<AdminSignupRequestDto>> GetRequestsAsync(
        string status)
    {
        return await _db.AccountRequests
            .AsNoTracking()
            .Where(r => r.Status == status)
            .OrderBy(r => r.RequestedAtUtc)
            .Select(r => new AdminSignupRequestDto
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId,
                FullName = r.FullName,
                Email = r.Email,
                DepartmentId = r.DepartmentId,
                DepartmentName = r.QMSDepartment == null
                    ? null
                    : r.QMSDepartment.DepartmentName,
                Role = r.Role,
                Status = r.Status,
                RequestedAtUtc = r.RequestedAtUtc,
                ReviewedByUserId = r.ReviewedByUserId,
                ReviewedAtUtc = r.ReviewedAtUtc
            })
            .ToListAsync();
    }

    public async Task<AdminSignupReviewResult> ReviewAsync(
        int id,
        string decision,
        int reviewerId)
    {
        var approved = decision.Equals(
            "Approve",
            StringComparison.OrdinalIgnoreCase);

        var rejected = decision.Equals(
            "Reject",
            StringComparison.OrdinalIgnoreCase);

        if (!approved && !rejected)
        {
            return AdminSignupReviewResult.Failure(
                400,
                "Decision must be Approve or Reject.");
        }

        var reviewerIsActiveAdmin = await _db.AppUsers.AnyAsync(u =>
            u.Id == reviewerId &&
            u.IsActive &&
            u.Role == QmsRole.QmsAdmin);

        if (!reviewerIsActiveAdmin)
        {
            return AdminSignupReviewResult.Failure(
                403,
                "Only an active QMS Admin can review signup requests.");
        }

        await using var transaction =
            await _db.Database.BeginTransactionAsync();

        var accountRequest = await _db.AccountRequests
            .FirstOrDefaultAsync(r => r.Id == id);

        if (accountRequest == null)
        {
            return AdminSignupReviewResult.Failure(
                404,
                "Signup request not found.");
        }

        if (accountRequest.Status != AccountRequestStatus.Pending)
        {
            return AdminSignupReviewResult.Failure(
                409,
                "Only pending signup requests can be reviewed.");
        }

        if (approved)
        {
            if (string.IsNullOrWhiteSpace(accountRequest.PasswordHash))
            {
                return AdminSignupReviewResult.Failure(
                    409,
                    "This request has no saved password hash. Ask the employee to submit a new signup request.");
            }

            if (accountRequest.Role != QmsRole.User &&
                accountRequest.Role != QmsRole.DepartmentHead)
            {
                return AdminSignupReviewResult.Failure(
                    409,
                    "The requested role is invalid.");
            }

            var departmentExists = await _db.QMSDepartments.AnyAsync(d =>
                d.DepartmentId == accountRequest.DepartmentId &&
                d.IsActive);

            if (!departmentExists)
            {
                return AdminSignupReviewResult.Failure(
                    409,
                    "The requested department is no longer active.");
            }

            var accountAlreadyExists = await _db.AppUsers.AnyAsync(u =>
                u.EmployeeId == accountRequest.EmployeeId ||
                u.Email == accountRequest.Email);

            if (accountAlreadyExists)
            {
                return AdminSignupReviewResult.Failure(
                    409,
                    "An account already exists for this employee ID or email.");
            }

            var now = DateTime.UtcNow;

            var newUser = new AppUser
            {
                EmployeeId = accountRequest.EmployeeId,
                FullName = accountRequest.FullName,
                Email = accountRequest.Email,
                DepartmentId = accountRequest.DepartmentId,
                Role = accountRequest.Role,
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                PasswordHash = accountRequest.PasswordHash
            };

            _db.AppUsers.Add(newUser);

            accountRequest.Status = AccountRequestStatus.Approved;
            accountRequest.PasswordHash = null;
        }
        else
        {
            accountRequest.Status = AccountRequestStatus.Rejected;
            accountRequest.PasswordHash = null;
        }

        accountRequest.PasswordSetupTokenHash = null;
        accountRequest.PasswordSetupTokenExpiresAtUtc = null;
        accountRequest.ReviewedByUserId = reviewerId;
        accountRequest.ReviewedAtUtc = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            return AdminSignupReviewResult.Failure(
                409,
                "The request could not be processed because of a database conflict. Check whether this employee account already exists.");
        }

        return AdminSignupReviewResult.Success(
            accountRequest.Id,
            accountRequest.Status,
            approved
                ? "Signup request approved. The employee can now sign in with the password submitted during signup."
                : "Signup request rejected.");
    }
}
