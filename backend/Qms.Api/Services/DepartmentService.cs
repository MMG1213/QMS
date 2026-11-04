
using Microsoft.EntityFrameworkCore;
using Qms.Api.Data;
using Qms.Api.DTOs;
using Qms.Api.Models;

namespace Qms.Api.Services;

public class DepartmentService : IDepartmentService
{
    private readonly QmsDbContext _db;

    public DepartmentService(QmsDbContext db)
    {
        _db = db;
    }

    public async Task<List<DepartmentAdminDto>> GetAllDepartmentsAsync()
    {
        return await _db.QMSDepartments
            .AsNoTracking()
            .OrderBy(d => d.DepartmentName)
            .Select(d => new DepartmentAdminDto
            {
                DepartmentId = d.DepartmentId,
                DepartmentCode = d.DepartmentCode,
                DepartmentName = d.DepartmentName,
                IsActive = d.IsActive
            })
            .ToListAsync();
    }

    public async Task<DepartmentAdminDto?> GetDepartmentByIdAsync(int id)
    {
        return await _db.QMSDepartments
            .AsNoTracking()
            .Where(d => d.DepartmentId == id)
            .Select(d => new DepartmentAdminDto
            {
                DepartmentId = d.DepartmentId,
                DepartmentCode = d.DepartmentCode,
                DepartmentName = d.DepartmentName,
                IsActive = d.IsActive
            })
            .SingleOrDefaultAsync();
    }

    public async Task<DepartmentOperationResult> CreateDepartmentAsync(
        CreateDepartmentRequest request)
    {
        var departmentCode =
            request.DepartmentCode?.Trim() ?? string.Empty;

        var departmentName =
            request.DepartmentName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(departmentCode) ||
            string.IsNullOrWhiteSpace(departmentName))
        {
            return DepartmentOperationResult.Failure(
                400,
                "Department code and department name are required.");
        }

        if (departmentCode.Length > 20 ||
            departmentName.Length > 100)
        {
            return DepartmentOperationResult.Failure(
                400,
                "Department code must be at most 20 characters and department name at most 100 characters.");
        }

        var duplicateExists = await _db.QMSDepartments.AnyAsync(d =>
            d.DepartmentCode == departmentCode ||
            d.DepartmentName == departmentName);

        if (duplicateExists)
        {
            return DepartmentOperationResult.Failure(
                409,
                "A department with that code or name already exists.");
        }

        var now = DateTime.UtcNow;

        var department = new QMSDepartment
        {
            DepartmentCode = departmentCode,
            DepartmentName = departmentName,
            IsActive = true,
            CreatedAtUtc = now
        };

        _db.QMSDepartments.Add(department);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return DepartmentOperationResult.Failure(
                409,
                "The department could not be created because its code or name may already exist.");
        }

        var response = new DepartmentAdminDto
        {
            DepartmentId = department.DepartmentId,
            DepartmentCode = department.DepartmentCode,
            DepartmentName = department.DepartmentName,
            IsActive = department.IsActive
        };

        return DepartmentOperationResult.Success(
            response,
            "Department created successfully.",
            201);
    }
    
    public async Task<DepartmentOperationResult> SetDepartmentStatusAsync(
        int id,
        bool isActive)
    {
        var department = await _db.QMSDepartments
            .SingleOrDefaultAsync(d => d.DepartmentId == id);

        if (department == null)
        {
            return DepartmentOperationResult.Failure(
                404,
                "Department not found.");
        }

        if (department.IsActive != isActive)
        {
            // When restricting a department, revoke all existing user sessions.
            if (!isActive)
            {
                var departmentUsers = await _db.AppUsers
                    .Where(u => u.DepartmentId == department.DepartmentId)
                    .ToListAsync();

                foreach (var user in departmentUsers)
                {
                    user.TokenVersion++;
                }
            }

            department.IsActive = isActive;
            department.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        var response = new DepartmentAdminDto
        {
            DepartmentId = department.DepartmentId,
            DepartmentCode = department.DepartmentCode,
            DepartmentName = department.DepartmentName,
            IsActive = department.IsActive
        };

        var message = isActive
            ? "Department activated successfully."
            : "Department restricted successfully.";

        return DepartmentOperationResult.Success(response, message);
    }
}
