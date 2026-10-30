using Qms.Api.DTOs;

namespace Qms.Api.Services;

public interface IDepartmentService
{
    Task<List<DepartmentAdminDto>> GetAllDepartmentsAsync();

    Task<DepartmentAdminDto?> GetDepartmentByIdAsync(int id);

    Task<DepartmentOperationResult> CreateDepartmentAsync(
        CreateDepartmentRequest request);

    Task<DepartmentOperationResult> SetDepartmentStatusAsync(
        int id,
        bool isActive);
}

public class DepartmentOperationResult
{
    public bool Succeeded { get; private set; }
    public int StatusCode { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public DepartmentAdminDto? Department { get; private set; }

    public static DepartmentOperationResult Success(
        DepartmentAdminDto department,
        string message,
        int statusCode = 200)
    {
        return new DepartmentOperationResult
        {
            Succeeded = true,
            StatusCode = statusCode,
            Message = message,
            Department = department
        };
    }

    public static DepartmentOperationResult Failure(
        int statusCode,
        string message)
    {
        return new DepartmentOperationResult
        {
            Succeeded = false,
            StatusCode = statusCode,
            Message = message
        };
    }
}