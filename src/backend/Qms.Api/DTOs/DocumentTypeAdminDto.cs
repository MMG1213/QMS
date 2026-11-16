
namespace Qms.Api.DTOs;

public class DocumentTypeAdminDto
{
    public int DocumentTypeId { get; set; }

    public string TypeCode { get; set; } = string.Empty;

    public string TypeName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
