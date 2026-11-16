
using Qms.Api.DTOs.DepartmentApprovalMappings;

namespace Qms.Api.Services.Interfaces;

public interface IDepartmentApprovalMappingService
{
    Task<List<DepartmentApprovalMappingDto>> GetAllAsync();

    Task<DepartmentApprovalMappingDto?> GetByIdAsync(int id);

    Task<List<ApprovalMappingEmployeeDto>> GetEmployeesAsync(
        int departmentId);

    Task<(bool Success, string Message, int? MappingId, int? ConfigVersion)>
        CreateAsync(CreateDepartmentApprovalMappingDto dto,
            string createdByEmployeeId);

    Task<(bool Success, string Message)> UpdateAsync(
        int id,
        UpdateDepartmentApprovalMappingDto dto);

    Task<(bool Success, string Message)> DeleteAsync(int id);
}
