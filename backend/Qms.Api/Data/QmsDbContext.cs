
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using Qms.Api.Models;

namespace Qms.Api.Data;

public class QmsDbContext : DbContext
{
    public QmsDbContext(
        DbContextOptions<QmsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<DepartmentApprovalMapping> DepartmentApprovalMappings
        => Set<DepartmentApprovalMapping>();

    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AccountRequest> AccountRequests => Set<AccountRequest>();

   public  DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentRevision> DocumentRevisions => Set<DocumentRevision>();

   public  DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
   public  DbSet<QMSDepartment> QMSDepartments => Set<QMSDepartment>();
   public DbSet<PasswordResetToken> PasswordResetTokens
    => Set<PasswordResetToken>();

    public DbSet<MachinesList> MachinesLists => Set<MachinesList>();

    public DbSet<Standards> Standards => Set<Standards>();

    public DbSet<TestingOrMeasuringInstruments> TestingOrMeasuringInstruments => Set<TestingOrMeasuringInstruments>();
    
    public DbSet<QualityPerformanceIndex> QualityPerformanceIndex => Set<QualityPerformanceIndex>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(d => d.Id);

            entity.Property(d => d.DeptName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(d => d.DeptId)
                .HasMaxLength(10)
                .IsRequired();

            entity.HasIndex(d => d.DeptName)
                .IsUnique();

            entity.HasIndex(d => d.DeptId)
                .IsUnique();
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.EmployeeId)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(u => u.FullName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(u => u.Email)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(u => u.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(u => u.Role)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(u => u.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(u => u.EmployeeId)
                .IsUnique();

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.HasOne(u => u.Department)
                .WithMany()
                .HasForeignKey(u => u.DepartmentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DepartmentApprovalMapping>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.PrimaryApproverEmployeeId)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(m => m.RedirectedApproverEmployeeId)
                .HasMaxLength(50);

            entity.HasOne(m => m.Department)
                .WithMany()
                .HasForeignKey(m => m.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(m => m.DepartmentId)
                .IsUnique();
        });

        modelBuilder.Entity<QMSDepartment>(entity =>
        {
            entity.HasKey(d => d.DepartmentId);

            entity.Property(d => d.DepartmentCode).HasMaxLength(20).IsRequired();

            entity.HasIndex(d => d.DepartmentCode).IsUnique();

            entity.Property(d => d.DepartmentName).HasMaxLength(100).IsRequired();
            entity.HasIndex(d => d.DepartmentName).IsUnique();
        });

        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.HasKey(t => t.DocumentTypeId);
            entity.Property(t => t.TypeCode).HasMaxLength(20).IsRequired();
            entity.Property(t => t.TypeName).HasMaxLength(100).IsRequired();
            entity.HasIndex(t => t.TypeCode).IsUnique();
        });


        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(d => d.DocumentId);

            entity.Property(d => d.DocumentNo)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(d => d.Title)
                .HasMaxLength(250)
                .IsRequired();

            entity.HasOne(d => d.QMSDepartment)
                .WithMany(department => department.Documents)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.DocumentType)
                .WithMany(type => type.Documents)
                .HasForeignKey(d => d.DocumentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

     entity.Property(d => d.RetentionPeriod).HasMaxLength(50)
     .IsRequired(false);

     entity.Property(d => d.Status).HasMaxLength(20).HasDefaultValue("Active")
     .IsRequired();

     entity.HasIndex(d => d.DocumentNo)
         .IsUnique();
 });

        modelBuilder.Entity<DocumentRevision>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.RevisionNumber)
                .IsRequired();

            entity.Property(r => r.PageCount)
                .IsRequired();

            entity.Property(r => r.RevisionDate)
                .HasColumnType("date")
                .IsRequired();

            entity.HasOne(r => r.Document)
                .WithMany(d => d.Revisions)
                .HasForeignKey(r => r.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => new
            {
                r.DocumentId,
                r.RevisionNumber
            }).IsUnique();
        });

        modelBuilder.Entity<AccountRequest>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.EmployeeId)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(r => r.FullName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(r => r.Email)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(r => r.Status)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne(r => r.Department)
                .WithMany()
                .HasForeignKey(r => r.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ReviewedByUser)
                .WithMany()
                .HasForeignKey(r => r.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => r.Email);

            entity.HasIndex(r => r.EmployeeId);

            entity.HasIndex(r => r.Status);

            entity.Property(r => r.PasswordSetupTokenHash)
                .HasMaxLength(64);

            entity.Property(r => r.PasswordSetupTokenExpiresAtUtc);

            entity.Property(r => r.Role)
                .HasMaxLength(30)
                .HasDefaultValue(QmsRole.User)
                .IsRequired();

            entity.Property(r => r.PasswordHash)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.TokenHash)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(t => t.CreatedAtUtc)
                .IsRequired();

            entity.Property(t => t.ExpiresAtUtc)
                .IsRequired();

            entity.HasIndex(t => t.TokenHash)
                .IsUnique();

            entity.HasIndex(t => new
            {
                t.UserId,
                t.ExpiresAtUtc
            });

            entity.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MachinesList>(entity =>
        {
           entity.HasKey(m => m.Id);

           entity.Property(m => m.MachineName)
               .HasMaxLength(150);
            
            entity.Property(m => m.MachineCategory)
                .HasMaxLength(50);

            entity.Property(m => m.MachineCodeNum)
                .HasMaxLength(50);
            
            entity.Property(m => m.MachineSlNum)
                .HasMaxLength(50);

            entity.Property(m => m.Make)
                .HasMaxLength(100);

            entity.Property(m => m.CapacityOrSpecification)
                .HasMaxLength(500);

            entity.Property(m => m.CommissionedOn)
                .HasColumnType("date");

            entity.Property(m => m.LoadInHP)
                .HasPrecision(10, 2);
            
            entity.Property(m => m.Location)
                .HasMaxLength(100);

            entity.HasOne(d => d.DocumentType).WithMany().HasForeignKey(d => d.DocumentTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Standards>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Description)
                .HasMaxLength(250);

            entity.Property(m => m.Pages);

            entity.Property(m => m.Revision);

            entity.Property(m => m.Remarks)
                .HasMaxLength(100);

            entity.HasOne(d => d.DocumentType).WithMany().HasForeignKey(d => d.DocumentTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TestingOrMeasuringInstruments>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.InstrumentDescription)
                .HasMaxLength(100);

            entity.Property(m => m.InstrumentCodeNum)
                .HasMaxLength(100);

            entity.Property(m => m.InstrumentSlNum)
                .HasMaxLength(50);
            
            entity.Property(m => m.InstrumentRange)
                .HasMaxLength(75);

            entity.Property(m => m.Make)
                .HasMaxLength(75);
            
            entity.Property(m => m.Location)
                .HasMaxLength(50);

            entity.Property(m => m.CalibrationFrequency)
                .HasMaxLength(50);
            
            entity.Property(m => m.Remarks)
                .HasMaxLength(100);
            
            entity.HasOne(d => d.DocumentType).WithMany().HasForeignKey(d => d.DocumentTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QualityPerformanceIndex>(entity =>
        {
            entity.ToTable("QualityPerformanceIndex");

            entity.HasKey(m => m.Id);

            entity.Property(m => m.ProcessOrDept)
                .HasMaxLength(100);

            entity.Property(m => m.MeasurementTitle)
                .HasMaxLength(150);

            entity.Property(m => m.QualityPerformanceIndexFormula)
                .HasMaxLength(500);

            entity.Property(m => m.Targets)
                .HasMaxLength(50);

            entity.Property(m => m.Responsibilities)
                .HasMaxLength(100);

            entity.Property(m => m.DepartmentReview)
                .HasMaxLength(20);

            entity.Property(m => m.QPIReview)
                .HasMaxLength(20);

            entity.HasOne(m => m.DocumentType)
                .WithMany()
                .HasForeignKey(m => m.DocumentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
