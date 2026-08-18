using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence.Configurations
{
    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Action)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(50);

            builder.Property(p => p.Code)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(100);

            builder.HasIndex(p => p.Code)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            builder.Property(p => p.Description)
                .IsUnicode(false)
                .HasMaxLength(256);

            builder.Property(p => p.IsDeleted)
                .HasDefaultValue(false);

            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // FK: Module (configured in ModuleConfiguration via HasMany)

            // Seed default permissions for User Management module
            var userMgmtModuleId = Guid.Parse("a1b2c3d4-e5f6-4a5b-8c7d-9e0f1a2b3c4d");
            var deptMgmtModuleId = Guid.Parse("b2c3d4e5-f6a7-4b5c-9d8e-0f1a2b3c4d5e");
            var roleMgmtModuleId = Guid.Parse("c3d4e5f6-a7b8-4c5d-0e9f-1a2b3c4d5e6f");
            var taskMgmtModuleId = Guid.Parse("d4e5f6a7-b8c9-4d5e-a000-2b3c4d5e6f7a");
            var projectMgmtModuleId = Guid.Parse("e5f6a7b8-c9d0-4e5f-b000-3c4d5e6f7a8b");

            var hrMgmtModuleId = Guid.Parse("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8b");
            var leaveMgmtModuleId = Guid.Parse("a7b8c9d0-e1f2-4a3b-b000-5c4d5e6f7a8b");

            builder.HasData(
                // User Management permissions
                new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), ModuleId = userMgmtModuleId, Action = "Create", Code = "user.create", Description = "Create new users", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), ModuleId = userMgmtModuleId, Action = "Read", Code = "user.read", Description = "View user details", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), ModuleId = userMgmtModuleId, Action = "Update", Code = "user.update", Description = "Edit user details", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), ModuleId = userMgmtModuleId, Action = "Delete", Code = "user.delete", Description = "Delete users", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

                // Department Management permissions
                new Permission { Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), ModuleId = deptMgmtModuleId, Action = "Create", Code = "department.create", Description = "Create departments", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("20000000-0000-0000-0000-000000000002"), ModuleId = deptMgmtModuleId, Action = "Read", Code = "department.read", Description = "View departments", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("20000000-0000-0000-0000-000000000003"), ModuleId = deptMgmtModuleId, Action = "Update", Code = "department.update", Description = "Edit departments", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("20000000-0000-0000-0000-000000000004"), ModuleId = deptMgmtModuleId, Action = "Delete", Code = "department.delete", Description = "Delete departments", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

                // Role Management permissions
                new Permission { Id = Guid.Parse("30000000-0000-0000-0000-000000000001"), ModuleId = roleMgmtModuleId, Action = "Create", Code = "role.create", Description = "Create roles", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("30000000-0000-0000-0000-000000000002"), ModuleId = roleMgmtModuleId, Action = "Read", Code = "role.read", Description = "View roles", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("30000000-0000-0000-0000-000000000003"), ModuleId = roleMgmtModuleId, Action = "Update", Code = "role.update", Description = "Edit roles", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("30000000-0000-0000-0000-000000000004"), ModuleId = roleMgmtModuleId, Action = "Delete", Code = "role.delete", Description = "Delete roles", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("30000000-0000-0000-0000-000000000005"), ModuleId = userMgmtModuleId, Action = "ManagePermissions", Code = "permission.override.manage", Description = "Grant/revoke a specific user's permission overrides, independent of role-definition rights so it can be delegated and scoped via PBAC", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

                // Task Management permissions
                new Permission { Id = Guid.Parse("40000000-0000-0000-0000-000000000001"), ModuleId = taskMgmtModuleId, Action = "Create", Code = "task.create", Description = "Create tasks", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("40000000-0000-0000-0000-000000000002"), ModuleId = taskMgmtModuleId, Action = "Read", Code = "task.read", Description = "Read tasks", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("40000000-0000-0000-0000-000000000003"), ModuleId = taskMgmtModuleId, Action = "Update", Code = "task.update", Description = "Update tasks", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("40000000-0000-0000-0000-000000000004"), ModuleId = taskMgmtModuleId, Action = "Delete", Code = "task.delete", Description = "Delete tasks", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

                // Project Management permissions
                new Permission { Id = Guid.Parse("80000000-0000-0000-0000-000000000001"), ModuleId = projectMgmtModuleId, Action = "Create", Code = "project.create", Description = "Create projects", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("80000000-0000-0000-0000-000000000002"), ModuleId = projectMgmtModuleId, Action = "Read", Code = "project.read", Description = "Read projects", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("80000000-0000-0000-0000-000000000003"), ModuleId = projectMgmtModuleId, Action = "Update", Code = "project.update", Description = "Update projects", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("80000000-0000-0000-0000-000000000004"), ModuleId = projectMgmtModuleId, Action = "Delete", Code = "project.delete", Description = "Delete projects", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

                // HR / Salary Management permissions
                new Permission { Id = Guid.Parse("50000000-0000-0000-0000-000000000001"), ModuleId = hrMgmtModuleId, Action = "Read", Code = "salary.read", Description = "Read employee salary and HR details", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("50000000-0000-0000-0000-000000000002"), ModuleId = hrMgmtModuleId, Action = "Write", Code = "salary.write", Description = "Modify employee salary and HR details", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

                // Leave Management permissions
                new Permission { Id = Guid.Parse("60000000-0000-0000-0000-000000000001"), ModuleId = leaveMgmtModuleId, Action = "Read", Code = "leave.read", Description = "Read leave requests", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("60000000-0000-0000-0000-000000000002"), ModuleId = leaveMgmtModuleId, Action = "Write", Code = "leave.write", Description = "Submit leave requests", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("60000000-0000-0000-0000-000000000003"), ModuleId = leaveMgmtModuleId, Action = "Approve", Code = "leave.approve", Description = "Approve/Reject leave requests", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

                // CRM permissions
                new Permission { Id = Guid.Parse("70000000-0000-0000-0000-000000000001"), ModuleId = Guid.Parse("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8c"), Action = "Read", Code = "leads.read", Description = "Read CRM leads", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("70000000-0000-0000-0000-000000000002"), ModuleId = Guid.Parse("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8c"), Action = "Write", Code = "leads.write", Description = "Modify CRM leads", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

                // Accounts permissions
                new Permission { Id = Guid.Parse("90000000-0000-0000-0000-000000000001"), ModuleId = Guid.Parse("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8d"), Action = "Read", Code = "invoices.read", Description = "Read invoices", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("90000000-0000-0000-0000-000000000002"), ModuleId = Guid.Parse("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8d"), Action = "Write", Code = "invoices.write", Description = "Modify invoices", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },

                // Policy permissions
                new Permission { Id = Guid.Parse("91000000-0000-0000-0000-000000000001"), ModuleId = Guid.Parse("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8e"), Action = "Read", Code = "policy.read", Description = "Read access policies", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Permission { Id = Guid.Parse("91000000-0000-0000-0000-000000000002"), ModuleId = Guid.Parse("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8e"), Action = "Write", Code = "policy.manage", Description = "Modify access policies", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}
