namespace Qms.Api.Models;

public class DCRRequest
{
    public int Id { get; set; }

    public string ReferenceNumber { get; set; } = string.Empty;

    public DateOnly RequestDate { get; set; }

    public int DepartmentId { get; set; }
    public QMSDepartment? Department { get; set; }
    public string Standard { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;

    public int DocumentTypeId { get; set; }

    public DocumentType? DocumentType { get; set; }

    public int? DocumentId { get; set; }
    public Document? Document { get; set; }

    public string? DocumentTitle { get; set; }
    public string? DocumentNo { get; set; }

    public int? CurrentRevisionNumber { get; set; }
    public int? NewRevisionNumber { get; set; }

    public DateOnly? EffectiveDateofImplementation { get; set; }
    public int? ProcessOwnerId { get; set; }
    public AppUser? ProcessOwner { get; set; }

    public DateOnly? ApprovalDate { get; set; }

    public string? DescriptionBeforechange { get; set; }
    public string? DescriptionAfterchange { get; set; }
    public string? Reasonforchange { get; set; }

    public string? AttachmentPath { get; set; }

    // Employee who submitted the DCR
    public int CreatedByUserId { get; set; }
    public AppUser? CreatedByUser { get; set; }
    
      public string Status { get; set; } = "Submitted";

      public string?RejectionReason{ get; set; }
    // Quality Manager use
    public DateOnly? DateUpdatedIntoSystem { get; set; }

    // Audit timestamps
    public DateTime CreatedAt{ get; set; } = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "India Standard Time");
    public DateTime? UpdatedAt { get; set; } 
    public DateTime? FinalizedAt { get; set; } 

    // Concurrency protection
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    
}