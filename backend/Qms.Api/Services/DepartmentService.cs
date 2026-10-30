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

    public async Task<List<DepartmentAdminDto>>
        GetAllDepartmentsAsync()
    {
        return await _db.Departments
            .AsNoTracking()
            .OrderBy(d => d.DeptName)
            .Select(d => new DepartmentAdminDto
            {
                Id = d.Id,
                DeptName = d.DeptName,
                DeptId = d.DeptId,
                IsActive = d.IsActive
            })
            .ToListAsync();
    }

    public async Task<DepartmentAdminDto?>
        GetDepartmentByIdAsync(int id)
    {
        return await _db.Departments
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DepartmentAdminDto
            {
                Id = d.Id,
                DeptName = d.DeptName,
                DeptId = d.DeptId,
                IsActive = d.IsActive
            })
            .SingleOrDefaultAsync();
    }

    public async Task<DepartmentOperationResult>
        CreateDepartmentAsync(CreateDepartmentRequest request)
    {
        var deptName = request.DeptName?.Trim() ?? string.Empty;
        var deptId = request.DeptId?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(deptName) ||
            string.IsNullOrWhiteSpace(deptId))
        {
            return DepartmentOperationResult.Failure(
                400,
                "Department name and department ID are required.");
        }

        if (deptName.Length > 100 || deptId.Length > 10)
        {
            return DepartmentOperationResult.Failure(
                400,
                "Department name must be at most 100 characters and department ID at most 10 characters.");
        }

        var duplicateExists = await _db.Departments.AnyAsync(d =>
            d.DeptName == deptName || d.DeptId == deptId);

        if (duplicateExists)
        {
            return DepartmentOperationResult.Failure(
                409,
                "A department with that name or department ID already exists.");
        }

        var department = new Department
        {
            DeptName = deptName,
            DeptId = deptId,
            IsActive = true
        };

        _db.Departments.Add(department);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // The unique database indexes protect against concurrent
            // requests creating duplicate names or IDs.
            return DepartmentOperationResult.Failure(
                409,
                "The department could not be created because its name or ID may already exist.");
        }

        var response = new DepartmentAdminDto
        {
            Id = department.Id,
            DeptName = department.DeptName,
            DeptId = department.DeptId,
            IsActive = department.IsActive
        };

        return DepartmentOperationResult.Success(
            response,
            "Department created successfully.",
            201);
    }

    public async Task<DepartmentOperationResult>
        SetDepartmentStatusAsync(int id, bool isActive)
    {
        var department = await _db.Departments
            .SingleOrDefaultAsync(d => d.Id == id);

        if (department == null)
        {
            return DepartmentOperationResult.Failure(
                404,
                "Department not found.");
        }

        department.IsActive = isActive;

        await _db.SaveChangesAsync();

        var response = new DepartmentAdminDto
        {
            Id = department.Id,
            DeptName = department.DeptName,
            DeptId = department.DeptId,
            IsActive = department.IsActive
        };

        var message = isActive
            ? "Department activated successfully."
            : "Department deactivated successfully.";

        return DepartmentOperationResult.Success(
            response,
            message);
    }
}