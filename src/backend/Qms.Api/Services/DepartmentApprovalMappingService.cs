
using Microsoft.EntityFrameworkCore;
using Qms.Api.Data;
using Qms.Api.DTOs.DepartmentApprovalMappings;
using Qms.Api.Models;
using Qms.Api.Services.Interfaces;

namespace Qms.Api.Services.Implementations;

public class DepartmentApprovalMappingService
    : IDepartmentApprovalMappingService
{
    private readonly QmsDbContext _context;

    public DepartmentApprovalMappingService(QmsDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartmentApprovalMappingDto>> GetAllAsync()
    {
        var mappings = await GetMappingQuery()
            .OrderBy(x => x.DepartmentId)
            .ThenByDescending(x => x.ConfigVersion)
            .ToListAsync();

        return mappings.Select(MapToDto).ToList();
    }

    public async Task<DepartmentApprovalMappingDto?> GetByIdAsync(int id)
    {
        var mapping = await GetMappingQuery()
            .FirstOrDefaultAsync(x => x.Id == id);

        return mapping == null ? null : MapToDto(mapping);
    }

    public async Task<List<ApprovalMappingEmployeeDto>> GetEmployeesAsync(
        int departmentId)
    {
        var departmentExists = await _context.QMSDepartments
            .AnyAsync(x => x.DepartmentId == departmentId && x.IsActive);

        if (!departmentExists)
            throw new InvalidOperationException(
                "Department not found or inactive.");

        // Return every active login ID so approval-level and final-approval
        // dropdowns can select employees from any department.
        // The Angular UI should filter DCR Creators by the selected department,
        // and ValidateConfigurationAsync enforces that rule on the server.
        return await _context.AppUsers
            .Where(x => x.IsActive)
            .OrderBy(x => x.FullName)
            .Select(x => new ApprovalMappingEmployeeDto
            {
                EmployeeId = x.EmployeeId,
                FullName = x.FullName,
                Role = x.Role,
                DepartmentId = x.DepartmentId
            })
            .ToListAsync();
    }

    public async Task<(bool Success, string Message, int? MappingId,
        int? ConfigVersion)> CreateAsync(
        CreateDepartmentApprovalMappingDto dto,
        string createdByEmployeeId)
    {
        var department = await _context.QMSDepartments
            .FirstOrDefaultAsync(x =>
                x.DepartmentId == dto.DepartmentId && x.IsActive);

        if (department == null)
            return (false, "Invalid or inactive department.", null, null);

        var activeMappingExists =
            await _context.DepartmentApprovalMappings
                .AnyAsync(x =>
                    x.DepartmentId == dto.DepartmentId && x.IsActive);

        if (activeMappingExists)
            return (false,
                "An active mapping already exists for this department.",
                null, null);

        var validationMessage = await ValidateConfigurationAsync(
            dto.DepartmentId,
            dto.CreatorEmployeeIds,
            dto.ApprovalLevels);

        if (validationMessage != null)
            return (false, validationMessage, null, null);

        if (string.IsNullOrWhiteSpace(createdByEmployeeId))
            return (false, "Logged-in Employee ID is missing.", null, null);

        var creatorExists = await _context.AppUsers
            .AnyAsync(x =>
                x.EmployeeId == createdByEmployeeId && x.IsActive);

        if (!creatorExists)
            return (false, "Invalid logged-in employee.", null, null);

        var latestVersion = await _context.DepartmentApprovalMappings
            .Where(x => x.DepartmentId == dto.DepartmentId)
            .Select(x => (int?)x.ConfigVersion)
            .MaxAsync() ?? 0;

        var now = DateTime.UtcNow;

        var mapping = new DepartmentApprovalMapping
        {
            DepartmentId = dto.DepartmentId,
            ConfigVersion = latestVersion + 1,
            IsActive = true,
            CreatedByEmployeeId = createdByEmployeeId,
            CreatedAtUtc = now
        };

        mapping.DCRCreators = dto.CreatorEmployeeIds
            .Distinct()
            .Select(employeeId => new DepartmentDCRCreator
            {
                CreatorEmployeeId = employeeId,
                CreatedAtUtc = now
            })
            .ToList();

        mapping.ApprovalLevels = dto.ApprovalLevels
            .OrderBy(x => x.LevelNumber)
            .Select(level => new DepartmentApprovalLevel
            {
                LevelNumber = level.LevelNumber,
                ApprovalType = level.ApprovalType,
                ApproverEmployeeId = level.ApproverEmployeeId,
                RedirectedApproverEmployeeId =
                    level.RedirectedApproverEmployeeId,
                IsActive = level.IsActive,
                CreatedAtUtc = now
            })
            .ToList();

        _context.DepartmentApprovalMappings.Add(mapping);

        await _context.SaveChangesAsync();

        return (true,
            "Department approval mapping created successfully.",
            mapping.Id,
            mapping.ConfigVersion);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(
        int id,
        UpdateDepartmentApprovalMappingDto dto)
    {
        var mapping = await _context.DepartmentApprovalMappings
            .Include(x => x.DCRCreators)
            .Include(x => x.ApprovalLevels)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (mapping == null)
            return (false, "Department approval mapping not found.");

        var validationMessage = await ValidateConfigurationAsync(
            mapping.DepartmentId,
            dto.CreatorEmployeeIds,
            dto.ApprovalLevels);

        if (validationMessage != null)
            return (false, validationMessage);

        var now = DateTime.UtcNow;

        mapping.IsActive = dto.IsActive;
        mapping.UpdatedAtUtc = now;

        _context.DepartmentDCRCreators.RemoveRange(mapping.DCRCreators);
        _context.DepartmentApprovalLevels.RemoveRange(
            mapping.ApprovalLevels);

        mapping.DCRCreators = dto.CreatorEmployeeIds
            .Distinct()
            .Select(employeeId => new DepartmentDCRCreator
            {
                DepartmentApprovalMappingId = mapping.Id,
                CreatorEmployeeId = employeeId,
                CreatedAtUtc = now
            })
            .ToList();

        mapping.ApprovalLevels = dto.ApprovalLevels
            .OrderBy(x => x.LevelNumber)
            .Select(level => new DepartmentApprovalLevel
            {
                DepartmentApprovalMappingId = mapping.Id,
                LevelNumber = level.LevelNumber,
                ApprovalType = level.ApprovalType,
                ApproverEmployeeId = level.ApproverEmployeeId,
                RedirectedApproverEmployeeId =
                    level.RedirectedApproverEmployeeId,
                IsActive = level.IsActive,
                CreatedAtUtc = now
            })
            .ToList();

        await _context.SaveChangesAsync();

        return (true, "Department approval mapping updated successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var mapping = await _context.DepartmentApprovalMappings
            .FirstOrDefaultAsync(x => x.Id == id);

        if (mapping == null)
            return (false, "Department approval mapping not found.");

        _context.DepartmentApprovalMappings.Remove(mapping);

        await _context.SaveChangesAsync();

        return (true, "Department approval mapping deleted successfully.");
    }

    private IQueryable<DepartmentApprovalMapping> GetMappingQuery()
    {
        return _context.DepartmentApprovalMappings
            .Include(x => x.QMSDepartment)
            .Include(x => x.CreatedByEmployee)
            .Include(x => x.DCRCreators)
                .ThenInclude(x => x.CreatorEmployee)
            .Include(x => x.ApprovalLevels)
                .ThenInclude(x => x.ApproverEmployee)
            .Include(x => x.ApprovalLevels)
                .ThenInclude(x => x.RedirectedApproverEmployee);
    }

    private async Task<string?> ValidateConfigurationAsync(
        int departmentId,
        List<string> creatorEmployeeIds,
        List<CreateApprovalLevelDto> approvalLevels)
    {
        if (creatorEmployeeIds == null || creatorEmployeeIds.Count == 0)
            return "At least one DCR creator must be selected.";

        if (approvalLevels == null || approvalLevels.Count == 0)
            return "At least one approval level must be configured.";

        if (creatorEmployeeIds.Any(string.IsNullOrWhiteSpace))
            return "Creator Employee IDs cannot be empty.";

        if (creatorEmployeeIds.Distinct().Count() !=
            creatorEmployeeIds.Count)
            return "Duplicate creator Employee IDs are not allowed.";

        var creators = await _context.AppUsers
            .Where(x =>
                creatorEmployeeIds.Contains(x.EmployeeId) && x.IsActive)
            .ToListAsync();

        if (creators.Count != creatorEmployeeIds.Count)
            return "One or more selected creators are invalid or inactive.";

        if (creators.Any(x => x.DepartmentId != departmentId))
            return "All creators must belong to the selected department.";

        if (approvalLevels.Any(x => x.LevelNumber < 1))
            return "LevelNumber must be greater than zero.";

        if (approvalLevels.Select(x => x.LevelNumber).Distinct().Count()
            != approvalLevels.Count)
            return "Duplicate approval level numbers are not allowed.";

        var validTypes = new[] { "Department", "QMSFinal" };

        if (approvalLevels.Any(x => !validTypes.Contains(x.ApprovalType)))
            return "ApprovalType must be Department or QMSFinal.";

        if (approvalLevels.Any(x =>
            string.IsNullOrWhiteSpace(x.ApproverEmployeeId)))
            return "Approver Employee ID is required for every level.";

        var approverIds = approvalLevels
            .Select(x => x.ApproverEmployeeId)
            .Concat(approvalLevels
                .Where(x => !string.IsNullOrWhiteSpace(
                    x.RedirectedApproverEmployeeId))
                .Select(x => x.RedirectedApproverEmployeeId!))
            .Distinct()
            .ToList();

        var approvers = await _context.AppUsers
            .Where(x => approverIds.Contains(x.EmployeeId) && x.IsActive)
            .ToListAsync();

        if (approvers.Count != approverIds.Count)
            return "One or more approvers are invalid or inactive.";

        foreach (var level in approvalLevels)
        {
            var approver = approvers.FirstOrDefault(
                x => x.EmployeeId == level.ApproverEmployeeId);

            if (approver == null)
            {
                return $"Invalid approver Employee ID at level {level.LevelNumber}.";
            }

            // Approval-level approvers can belong to any department.
            // QMSFinal can also be assigned to any active login ID.
            // There is intentionally no department or QMS Admin role restriction.

            if (!string.IsNullOrWhiteSpace(
                level.RedirectedApproverEmployeeId))
            {
                var redirected = approvers.FirstOrDefault(
                    x => x.EmployeeId == level.RedirectedApproverEmployeeId);

                if (redirected == null)
                {
                    return $"Invalid redirected approver Employee ID at level {level.LevelNumber}.";
                }

                // No department or role restriction for redirected approvers.
            }
        }

        return null;
    }

    private static DepartmentApprovalMappingDto MapToDto(
        DepartmentApprovalMapping mapping)
    {
        return new DepartmentApprovalMappingDto
        {
            Id = mapping.Id,
            DepartmentId = mapping.DepartmentId,
            DepartmentName = mapping.QMSDepartment?.DepartmentName ?? "",
            ConfigVersion = mapping.ConfigVersion,
            IsActive = mapping.IsActive,

            CreatedByEmployeeId = mapping.CreatedByEmployeeId,
            CreatedByEmployeeName =
                mapping.CreatedByEmployee?.FullName ?? "",

            CreatedAtUtc = mapping.CreatedAtUtc,
            UpdatedAtUtc = mapping.UpdatedAtUtc,

            DCRCreators = mapping.DCRCreators
                .Select(x => new DCRCreatorDto
                {
                    CreatorEmployeeId = x.CreatorEmployeeId,
                    CreatorEmployeeName =
                        x.CreatorEmployee?.FullName ?? ""
                })
                .ToList(),

            ApprovalLevels = mapping.ApprovalLevels
                .OrderBy(x => x.LevelNumber)
                .Select(x => new ApprovalLevelDto
                {
                    Id = x.Id,
                    LevelNumber = x.LevelNumber,
                    ApprovalType = x.ApprovalType,

                    ApproverEmployeeId = x.ApproverEmployeeId,
                    ApproverEmployeeName =
                        x.ApproverEmployee?.FullName ?? "",

                    RedirectedApproverEmployeeId =
                        x.RedirectedApproverEmployeeId,

                    RedirectedApproverEmployeeName =
                        x.RedirectedApproverEmployee?.FullName,

                    IsActive = x.IsActive
                })
                .ToList()
        };
    }
}
