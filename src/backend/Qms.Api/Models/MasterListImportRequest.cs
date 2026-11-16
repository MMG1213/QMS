using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Qms.Api.Models;

public class MasterListImportRequest
{
    [Required]
    public IFormFile File { get; set; } = null!;
}