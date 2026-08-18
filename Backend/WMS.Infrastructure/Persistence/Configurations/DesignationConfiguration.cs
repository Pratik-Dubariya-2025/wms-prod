using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class DesignationConfiguration : IEntityTypeConfiguration<Designation>
    {
        public void Configure(EntityTypeBuilder<Designation> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(100);

            builder.Property(d => d.Code)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(20);

            builder.HasIndex(d => d.Code)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.Property(d => d.Description)
                .IsUnicode(false)
                .HasMaxLength(256);

            builder.Property(d => d.Level)
                .HasDefaultValue(1);

            builder.Property(d => d.IsActive)
                .HasDefaultValue(true);

            builder.Property(d => d.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(d => d.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // FK: Department
            builder.HasOne(d => d.Department)
                .WithMany(dep => dep.Designations)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed default designations
            builder.HasData(
                new Designation { Id = Guid.Parse("60000000-0000-0000-0000-000000000001"), DepartmentId = Guid.Parse("50000000-0000-0000-0000-000000000001"), Name = "Junior Software Engineer", Code = "JR_SE", Level = 1, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Designation { Id = Guid.Parse("60000000-0000-0000-0000-000000000002"), DepartmentId = Guid.Parse("50000000-0000-0000-0000-000000000001"), Name = "Technical Support Engineer", Code = "TSE", Level = 2, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Designation { Id = Guid.Parse("60000000-0000-0000-0000-000000000003"), DepartmentId = Guid.Parse("50000000-0000-0000-0000-000000000001"), Name = "Senior Software Engineer", Code = "SR_SE", Level = 3, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Designation { Id = Guid.Parse("60000000-0000-0000-0000-000000000004"), DepartmentId = Guid.Parse("50000000-0000-0000-0000-000000000001"), Name = "Tech Lead", Code = "TECH_LEAD", Level = 4, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Designation { Id = Guid.Parse("60000000-0000-0000-0000-000000000005"), DepartmentId = Guid.Parse("50000000-0000-0000-0000-000000000002"), Name = "HR Executive", Code = "HR_EXEC", Level = 1, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Designation { Id = Guid.Parse("60000000-0000-0000-0000-000000000006"), DepartmentId = Guid.Parse("50000000-0000-0000-0000-000000000002"), Name = "HR Manager", Code = "HR_MGR", Level = 3, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Designation { Id = Guid.Parse("60000000-0000-0000-0000-000000000007"), DepartmentId = Guid.Parse("50000000-0000-0000-0000-000000000003"), Name = "Accountant", Code = "ACCT", Level = 1, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Designation { Id = Guid.Parse("60000000-0000-0000-0000-000000000008"), DepartmentId = Guid.Parse("50000000-0000-0000-0000-000000000003"), Name = "Finance Manager", Code = "FIN_MGR", Level = 3, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Designation { Id = Guid.Parse("60000000-0000-0000-0000-000000000009"), DepartmentId = Guid.Parse("50000000-0000-0000-0000-000000000001"), Name = "Software Engineer", Code = "SE", Level = 2, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Designation { Id = Guid.Parse("60000000-0000-0000-0000-000000000010"), DepartmentId = Guid.Parse("50000000-0000-0000-0000-000000000001"), Name = "Project Manager", Code = "PM", Level = 5, IsActive = true, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}
