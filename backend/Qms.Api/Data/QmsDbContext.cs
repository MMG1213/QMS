
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
    }
}
