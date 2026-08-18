using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class ModuleConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Name)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(100);

            builder.HasIndex(m => m.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.Property(m => m.Code)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(50);

            builder.HasIndex(m => m.Code)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.Property(m => m.Description)
                .IsUnicode(false)
                .HasMaxLength(256);

            builder.Property(m => m.DisplayOrder)
                .HasDefaultValue(0);

            builder.Property(m => m.IsActive)
                .HasDefaultValue(true);

            builder.Property(m => m.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(m => m.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Nav: Permissions
            builder.HasMany(m => m.Permissions)
                .WithOne(p => p.Module)
                .HasForeignKey(p => p.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed default modules
            builder.HasData(
                new Module
                {
                    Id = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c7d-9e0f1a2b3c4d"),
                    Name = "User Management",
                    Code = "USER_MGMT",
                    Description = "Manage users, roles, and permissions",
                    DisplayOrder = 1,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Module
                {
                    Id = Guid.Parse("b2c3d4e5-f6a7-4b5c-9d8e-0f1a2b3c4d5e"),
                    Name = "Department Management",
                    Code = "DEPT_MGMT",
                    Description = "Manage departments and designations",
                    DisplayOrder = 2,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Module
                {
                    Id = Guid.Parse("c3d4e5f6-a7b8-4c5d-0e9f-1a2b3c4d5e6f"),
                    Name = "Role Management",
                    Code = "ROLE_MGMT",
                    Description = "Manage roles and assign permissions",
                    DisplayOrder = 3,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Module
                {
                    Id = Guid.Parse("d4e5f6a7-b8c9-4d5e-a000-2b3c4d5e6f7a"),
                    Name = "Task Management",
                    Code = "TASK_MGMT",
                    Description = "Manage tasks and status transitions",
                    DisplayOrder = 4,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Module
                {
                    Id = Guid.Parse("e5f6a7b8-c9d0-4e5f-b000-3c4d5e6f7a8b"),
                    Name = "Project Management",
                    Code = "PROJECT_MGMT",
                    Description = "Manage client projects and departments",
                    DisplayOrder = 5,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Module
                {
                    Id = Guid.Parse("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8b"),
                    Name = "HR Management",
                    Code = "HR_MGMT",
                    Description = "Manage employee profile and salaries",
                    DisplayOrder = 6,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Module
                {
                    Id = Guid.Parse("a7b8c9d0-e1f2-4a3b-b000-5c4d5e6f7a8b"),
                    Name = "Leave Management",
                    Code = "LEAVE_MGMT",
                    Description = "Manage employee leave requests",
                    DisplayOrder = 7,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Module
                {
                    Id = Guid.Parse("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8c"),
                    Name = "CRM",
                    Code = "CRM",
                    Description = "Manage client leads and pipeline stages",
                    DisplayOrder = 8,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Module
                {
                    Id = Guid.Parse("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8d"),
                    Name = "Accounts",
                    Code = "ACCOUNTS",
                    Description = "Manage financial invoices",
                    DisplayOrder = 9,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Module
                {
                    Id = Guid.Parse("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8e"),
                    Name = "Policy Management",
                    Code = "POLICY_MGMT",
                    Description = "Manage fine-grained access policies",
                    DisplayOrder = 10,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
