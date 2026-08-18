using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Name)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(100);

            builder.HasIndex(d => d.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

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

            builder.Property(d => d.IsActive)
                .HasDefaultValue(true);

            builder.Property(d => d.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(d => d.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Seed default departments
            builder.HasData(
                new Department
                {
                    Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                    Name = "Information Technology",
                    Code = "IT",
                    Description = "Software development, infrastructure, and technical support",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Department
                {
                    Id = Guid.Parse("50000000-0000-0000-0000-000000000002"),
                    Name = "Human Resources",
                    Code = "HR",
                    Description = "Employee management, recruitment, and compliance",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Department
                {
                    Id = Guid.Parse("50000000-0000-0000-0000-000000000003"),
                    Name = "Finance",
                    Code = "FIN",
                    Description = "Accounting, budgeting, and financial reporting",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Department
                {
                    Id = Guid.Parse("50000000-0000-0000-0000-000000000004"),
                    Name = "Operations",
                    Code = "OPS",
                    Description = "Day-to-day business operations and logistics",
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
