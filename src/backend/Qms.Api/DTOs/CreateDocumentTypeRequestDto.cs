
using System.ComponentModel.DataAnnotations;

namespace Qms.Api.DTOs;

public class CreateDocumentTypeRequest
{
    [Required]
    [StringLength(20)]
    public string TypeCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string TypeName { get; set; } = string.Empty;
}
