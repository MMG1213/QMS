using System.ComponentModel.DataAnnotations;
namespace Qms.Api.Dtos;

public class BulkObsoleteDocuments
{
    [Required]
    [MinLength(1, ErrorMessage = "Select atleast one document.")]
    public List<int> DocumentIds { get; set; } = new();
}