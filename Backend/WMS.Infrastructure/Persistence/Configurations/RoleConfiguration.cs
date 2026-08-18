using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(50);

            builder.HasIndex(r => r.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.Property(r => r.Code)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(50);

            builder.HasIndex(r => r.Code)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.Property(r => r.Priority)
                .HasDefaultValue(100);

            builder.Property(r => r.Description)
                .IsUnicode(false)
                .HasMaxLength(256);

            builder.Property(r => r.IsSystemRole)
                .HasDefaultValue(false);

            builder.Property(r => r.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Nav: RolePermissions
            builder.HasMany(r => r.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed system roles. Priority: lower number = higher authority (SRS 2.3 / 4.2.1).
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            builder.HasData(
                new Role
                {
                    Id = Guid.Parse("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98"),
                    Name = "ADMIN", Code = "ADMIN", Priority = 1,
                    Description = "Full system access, user management, global configuration",
                    IsSystemRole = true, CreatedAt = seedDate
                },
                new Role
                {
                    Id = Guid.Parse("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a"),
                    Name = "HR_MANAGER", Code = "HR", Priority = 10,
                    Description = "All employee PII, salary, leave, payroll across all departments",
                    IsSystemRole = true, CreatedAt = seedDate
                },
                new Role
                {
                    Id = Guid.Parse("a1000000-0000-0000-0000-000000000001"),
                    Name = "ACCOUNTS_MANAGER", Code = "ACCOUNTS", Priority = 10,
                    Description = "Invoices, expenses, payroll, financial reports",
                    IsSystemRole = true, CreatedAt = seedDate
                },
                new Role
                {
                    Id = Guid.Parse("a1000000-0000-0000-0000-000000000002"),
                    Name = "DEPARTMENT_MANAGER", Code = "MANAGER", Priority = 20,
                    Description = "All teams in their department, performance, approvals",
                    IsSystemRole = true, CreatedAt = seedDate
                },
                new Role
                {
                    Id = Guid.Parse("a1000000-0000-0000-0000-000000000006"),
                    Name = "BUSINESS_DEV_EXECUTIVE", Code = "BDE", Priority = 25,
                    Description = "Lead pipeline for their region",
                    IsSystemRole = true, CreatedAt = seedDate
                },
                new Role
                {
                    Id = Guid.Parse("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b"),
                    Name = "TEAM_LEAD", Code = "TL", Priority = 30,
                    Description = "Their team's tasks, timesheets, performance reviews",
                    IsSystemRole = true, CreatedAt = seedDate
                },
                new Role
                {
                    Id = Guid.Parse("a1000000-0000-0000-0000-000000000007"),
                    Name = "BUSINESS_DEV_ASSOCIATE", Code = "BDA", Priority = 35,
                    Description = "Own leads only, limited CRM access",
                    IsSystemRole = true, CreatedAt = seedDate
                },
                new Role
                {
                    Id = Guid.Parse("a1000000-0000-0000-0000-000000000003"),
                    Name = "SENIOR_SOFTWARE_ENGINEER", Code = "SSE", Priority = 40,
                    Description = "Own work plus can review junior engineer PRs",
                    IsSystemRole = true, CreatedAt = seedDate
                },
                new Role
                {
                    Id = Guid.Parse("a1000000-0000-0000-0000-000000000004"),
                    Name = "SOFTWARE_ENGINEER", Code = "SE", Priority = 50,
                    Description = "Own tasks and timesheets only",
                    IsSystemRole = true, CreatedAt = seedDate
                },
                new Role
                {
                    Id = Guid.Parse("a1000000-0000-0000-0000-000000000005"),
                    Name = "ASSOCIATE_SOFTWARE_ENGINEER", Code = "ASE_TSE", Priority = 60,
                    Description = "Own tasks only, read-only project visibility",
                    IsSystemRole = true, CreatedAt = seedDate
                },
                new Role
                {
                    Id = Guid.Parse("7c1d6d5f-f9d7-44c9-b7e2-5282f3f0ab09"),
                    Name = "EMPLOYEE", Code = "EMPLOYEE", Priority = 100,
                    Description = "Standard employee access",
                    IsSystemRole = true, CreatedAt = seedDate
                }
            );
        }
    }
}
