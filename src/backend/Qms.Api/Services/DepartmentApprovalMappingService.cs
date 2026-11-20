
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

    // GET ALL CONFIGURATIONS
    public async Task<List<DepartmentApprovalMappingDto>> GetAllAsync()
    {
        var mappings = await GetMappingQuery()
            .OrderBy(x => x.DepartmentId)
            .ThenByDescending(x => x.ConfigVersion)
            .ToListAsync();

        return mappings.Select(MapToDto).ToList();
    }

    // GET CONFIGURATION BY ID
    public async Task<DepartmentApprovalMappingDto?> GetByIdAsync(int id)
    {
        var mapping = await GetMappingQuery()
            .FirstOrDefaultAsync(x => x.Id == id);

        return mapping == null ? null : MapToDto(mapping);
    }

    // GET ACTIVE EMPLOYEES
    public async Task<List<ApprovalMappingEmployeeDto>> GetEmployeesAsync(
        int departmentId)
    {
        var departmentExists = await _context.QMSDepartments
            .AnyAsync(x =>
                x.DepartmentId == departmentId && x.IsActive);

        if (!departmentExists)
            throw new InvalidOperationException(
                "Department not found or inactive.");

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

    // CREATE NEW WORKFLOW CONFIGURATION
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
            return (false,
                "Logged-in Employee ID is missing.", null, null);

        var creatorExists = await _context.AppUsers
            .AnyAsync(x =>
                x.EmployeeId == createdByEmployeeId && x.IsActive);

        if (!creatorExists)
            return (false,
                "Invalid logged-in employee.", null, null);

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

    // UPDATE CONFIGURATION BY CREATING A NEW VERSION
    public async Task<(bool Success, string Message)> UpdateAsync(
        int id,
        UpdateDepartmentApprovalMappingDto dto,
        string createdByEmployeeId)
    {
        if (string.IsNullOrWhiteSpace(createdByEmployeeId))
            return (false, "Logged-in Employee ID is missing.");

        var adminExists = await _context.AppUsers
            .AnyAsync(x =>
                x.EmployeeId == createdByEmployeeId && x.IsActive);

        if (!adminExists)
            return (false, "Invalid logged-in employee.");

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);

        try
        {
            var mapping = await _context.DepartmentApprovalMappings
                .FirstOrDefaultAsync(x => x.Id == id);

            if (mapping == null)
                return (false,
                    "Department approval mapping not found.");

            if (!mapping.IsActive)
                return (false,
                    "Historical configurations cannot be edited. Edit the active configuration instead.");

            var now = DateTime.UtcNow;

            // CASE 1: DEACTIVATE THE CURRENT CONFIGURATION
            if (!dto.IsActive)
            {
                mapping.IsActive = false;
                mapping.UpdatedAtUtc = now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true,
                    "Department approval mapping deactivated successfully.");
            }

            // CASE 2: CREATE A NEW CONFIGURATION VERSION
            var validationMessage = await ValidateConfigurationAsync(
                mapping.DepartmentId,
                dto.CreatorEmployeeIds,
                dto.ApprovalLevels);

            if (validationMessage != null)
                return (false, validationMessage);

            var latestVersion =
                await _context.DepartmentApprovalMappings
                    .Where(x => x.DepartmentId == mapping.DepartmentId)
                    .Select(x => (int?)x.ConfigVersion)
                    .MaxAsync() ?? 0;

            // Deactivate the previous version.
            mapping.IsActive = false;
            mapping.UpdatedAtUtc = now;

            await _context.SaveChangesAsync();

            // Create a new mapping record.
            var newMapping = new DepartmentApprovalMapping
            {
                DepartmentId = mapping.DepartmentId,
                ConfigVersion = latestVersion + 1,
                IsActive = true,
                CreatedByEmployeeId = createdByEmployeeId,
                CreatedAtUtc = now
            };

            // Create new DCR creator records.
            newMapping.DCRCreators = dto.CreatorEmployeeIds
                .Distinct()
                .Select(employeeId => new DepartmentDCRCreator
                {
                    CreatorEmployeeId = employeeId,
                    CreatedAtUtc = now
                })
                .ToList();

            // Create new approval-level records.
            newMapping.ApprovalLevels = dto.ApprovalLevels
                .OrderBy(x => x.LevelNumber)
                .Select(level => new DepartmentApprovalLevel
                {
                    LevelNumber = level.LevelNumber,
                    ApprovalType = level.ApprovalType,
                    ApproverEmployeeId = level.ApproverEmployeeId,
                    IsActive = level.IsActive,
                    CreatedAtUtc = now
                })
                .ToList();

            _context.DepartmentApprovalMappings.Add(newMapping);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return (true,
                $"New workflow configuration version {newMapping.ConfigVersion} created successfully.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // DEACTIVATE CONFIGURATION INSTEAD OF PHYSICALLY DELETING IT
    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var mapping = await _context.DepartmentApprovalMappings
            .FirstOrDefaultAsync(x => x.Id == id);

        if (mapping == null)
            return (false, "Department approval mapping not found.");

        if (!mapping.IsActive)
            return (true,
                "Department approval mapping is already inactive.");

        mapping.IsActive = false;
        mapping.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return (true,
            "Department approval mapping deactivated successfully.");
    }

    // COMMON QUERY FOR LOADING CONFIGURATION DETAILS
    private IQueryable<DepartmentApprovalMapping> GetMappingQuery()
    {
        return _context.DepartmentApprovalMappings
            .Include(x => x.QMSDepartment)
            .Include(x => x.CreatedByEmployee)
            .Include(x => x.DCRCreators)
                .ThenInclude(x => x.CreatorEmployee)
            .Include(x => x.ApprovalLevels)
                .ThenInclude(x => x.ApproverEmployee);
    }

    // VALIDATE WORKFLOW CONFIGURATION
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
            .Distinct()
            .ToList();

        var approvers = await _context.AppUsers
            .Where(x =>
                approverIds.Contains(x.EmployeeId) && x.IsActive)
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

            // Approvers can belong to any department.
            // No department or role restriction is applied.
        }

        return null;
    }

    // MAP ENTITY TO DTO
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

                    IsActive = x.IsActive
                })
                .ToList()
        };
    }
}
