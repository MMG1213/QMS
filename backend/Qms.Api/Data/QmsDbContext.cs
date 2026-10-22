
using System.Diagnostics.SymbolStore;
using System.Dynamic;
//using System.Reflection.Metadata;
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

    //Master list models

   public  DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentRevision> DocumentRevisions => Set<DocumentRevision>();

   public  DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
   public  DbSet<QMSDepartment> QMSDepartments => Set<QMSDepartment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(d => d.Id);

            entity.Property(d => d.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(d => d.Name)
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

            entity.HasIndex(u => u.EmployeeId)
                .IsUnique();

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.HasOne(u => u.Department)
                .WithMany()
                .HasForeignKey(u => u.DepartmentId)
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
    }
}
