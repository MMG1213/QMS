
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Qms.Api.Data;
using Qms.Api.DTOs;
using Qms.Api.Models;

namespace Qms.Api.Services;

public class SignupService : ISignupService
{
    private readonly QmsDbContext _db;
    private readonly IPasswordHasher<AppUser> _passwordHasher;

    public SignupService(
        QmsDbContext db,
        IPasswordHasher<AppUser> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<SignupServiceResult> SubmitAsync(
        SignupRequestDto request)
    {
        var employeeId = request.EmployeeId.Trim();
        var fullName = request.FullName.Trim();
        var email = request.Email.Trim();
        var role = request.Role.Trim();

        if (string.IsNullOrWhiteSpace(employeeId) ||
            string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email))
        {
            return SignupServiceResult.Failure(
                400,
                "Employee ID, full name, and email are required.");
        }

        if (role != QmsRole.User &&
            role != QmsRole.DepartmentHead)
        {
            return SignupServiceResult.Failure(
                400,
                "Requested role must be User or Department Head.");
        }

        var departmentExists = await _db.Departments.AnyAsync(d =>
            d.Id == request.DepartmentId && d.IsActive);

        if (!departmentExists)
        {
            return SignupServiceResult.Failure(
                400,
                "Select a valid active department.");
        }

        var accountExists = await _db.AppUsers.AnyAsync(u =>
            u.EmployeeId == employeeId || u.Email == email);

        if (accountExists)
        {
            return SignupServiceResult.Failure(
                409,
                "An account with that employee ID or email already exists.");
        }

        var pendingRequestExists = await _db.AccountRequests.AnyAsync(r =>
            r.Status == AccountRequestStatus.Pending &&
            (r.EmployeeId == employeeId || r.Email == email));

        if (pendingRequestExists)
        {
            return SignupServiceResult.Failure(
                409,
                "A pending signup request already exists for that employee ID or email.");
        }

        // Hash the password immediately; never store plaintext.
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
            return SignupServiceResult.Failure(
                409,
                "The signup request could not be saved because of a data conflict.");
        }

        return SignupServiceResult.Success(
            accountRequest.Id,
            accountRequest.Status.ToString(),
            "Signup request submitted for QMS Admin review.");
    }
}
