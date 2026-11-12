using System.ComponentModel.DataAnnotations;

namespace Qms.Api.Dtos;

public class UpdateMasterListDto
{
    [Required]
    [StringLength(50)]
    public string DocumentNo { get; set; } = string.Empty;
    [Required]
    [StringLength(250)]
    public string Title { get; set; } = string.Empty;
    
    [Range(1,int.MaxValue)]
    public int DocumentTypeId { get; set; }
    
    [Range(1,int.MaxValue)]
    public int DepartmentId { get; set; }

    [StringLength(50)]
    public string? RetentionPeriod { get; set; }

    [Required]
    [RegularExpression("^(Active|Obsolete)$")]
    public string Status { get; set; } = "Active";

    public DateOnly? ObseleteDate { get; set; }

    public List<UpdateDocumentRevisionDto> Revisions{ get; set; } = new();

}

public class UpdateDocumentRevisionDto
{
    [Range(0, int.MaxValue)]
    public int RevisionNumber { get; set; }

    [Range(1, int.MaxValue)]
    public int PageCount { get; set; }
    
    public DateTime RevisionDate{ get; set; }
}