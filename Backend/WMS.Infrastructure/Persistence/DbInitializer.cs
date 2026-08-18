using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WMS.Domain.Models;

namespace WMS.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        private static readonly DateTime SeedTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static void Initialize(AppDbContext context)
        {
            // Ensure database is migrated
            context.Database.Migrate();

            // 1. Seed Modules
            var hrModuleId = Guid.Parse("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8b");
            var leaveModuleId = Guid.Parse("a7b8c9d0-e1f2-4a3b-b000-5c4d5e6f7a8b");
            var policyModuleId = Guid.Parse("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8e");

            var modules = new List<Module>
            {
                new Module
                {
                    Id = hrModuleId,
                    Name = "HR Management",
                    Code = "HR_MGMT",
                    Description = "Manage employee profile and salaries",
                    DisplayOrder = 6,
                    CreatedAt = SeedTime,
                    IsActive = true
                },
                new Module
                {
                    Id = leaveModuleId,
                    Name = "Leave Management",
                    Code = "LEAVE_MGMT",
                    Description = "Manage employee leave requests",
                    DisplayOrder = 7,
                    CreatedAt = SeedTime,
                    IsActive = true
                },
                new Module
                {
                    Id = policyModuleId,
                    Name = "Policy Management",
                    Code = "POLICY_MGMT",
                    Description = "Manage fine-grained access policies",
                    DisplayOrder = 10,
                    CreatedAt = SeedTime,
                    IsActive = true
                }
            };

            foreach (var module in modules)
            {
                if (!context.Modules.Any(m => m.Id == module.Id))
                {
                    context.Modules.Add(module);
                }
            }
            context.SaveChanges();

            // 2. Seed Permissions
            var crmModuleId = Guid.Parse("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8c");
            var accountsModuleId = Guid.Parse("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8d");

            var permissions = new List<Permission>
            {
                // HR / Salary Management permissions
                new Permission 
                { 
                    Id = Guid.Parse("50000000-0000-0000-0000-000000000001"), 
                    ModuleId = hrModuleId, 
                    Action = "Read", 
                    Code = "salary.read", 
                    Description = "Read employee salary and HR details", 
                    CreatedAt = SeedTime 
                },
                new Permission 
                { 
                    Id = Guid.Parse("50000000-0000-0000-0000-000000000002"), 
                    ModuleId = hrModuleId, 
                    Action = "Write", 
                    Code = "salary.write", 
                    Description = "Modify employee salary and HR details", 
                    CreatedAt = SeedTime 
                },

                // Leave Management permissions
                new Permission 
                { 
                    Id = Guid.Parse("60000000-0000-0000-0000-000000000001"), 
                    ModuleId = leaveModuleId, 
                    Action = "Read", 
                    Code = "leave.read", 
                    Description = "Read leave requests", 
                    CreatedAt = SeedTime 
                },
                new Permission 
                { 
                    Id = Guid.Parse("60000000-0000-0000-0000-000000000002"), 
                    ModuleId = leaveModuleId, 
                    Action = "Write", 
                    Code = "leave.write", 
                    Description = "Submit leave requests", 
                    CreatedAt = SeedTime 
                },
                new Permission 
                { 
                    Id = Guid.Parse("60000000-0000-0000-0000-000000000003"), 
                    ModuleId = leaveModuleId, 
                    Action = "Approve", 
                    Code = "leave.approve", 
                    Description = "Approve/Reject leave requests", 
                    CreatedAt = SeedTime 
                },

                // CRM permissions
                new Permission
                {
                    Id = Guid.Parse("70000000-0000-0000-0000-000000000001"),
                    ModuleId = crmModuleId,
                    Action = "Read",
                    Code = "leads.read",
                    Description = "Read CRM leads",
                    CreatedAt = SeedTime
                },
                new Permission
                {
                    Id = Guid.Parse("70000000-0000-0000-0000-000000000002"),
                    ModuleId = crmModuleId,
                    Action = "Write",
                    Code = "leads.write",
                    Description = "Modify CRM leads",
                    CreatedAt = SeedTime
                },

                // Accounts permissions
                new Permission
                {
                    Id = Guid.Parse("90000000-0000-0000-0000-000000000001"),
                    ModuleId = accountsModuleId,
                    Action = "Read",
                    Code = "invoices.read",
                    Description = "Read invoices",
                    CreatedAt = SeedTime
                },
                new Permission
                {
                    Id = Guid.Parse("90000000-0000-0000-0000-000000000002"),
                    ModuleId = accountsModuleId,
                    Action = "Write",
                    Code = "invoices.write",
                    Description = "Modify invoices",
                    CreatedAt = SeedTime
                },

                // Policy permissions
                new Permission
                {
                    Id = Guid.Parse("91000000-0000-0000-0000-000000000001"),
                    ModuleId = policyModuleId,
                    Action = "Read",
                    Code = "policy.read",
                    Description = "Read access policies",
                    CreatedAt = SeedTime
                },
                new Permission
                {
                    Id = Guid.Parse("91000000-0000-0000-0000-000000000002"),
                    ModuleId = policyModuleId,
                    Action = "Write",
                    Code = "policy.manage",
                    Description = "Modify access policies",
                    CreatedAt = SeedTime
                }
            };

            foreach (var permission in permissions)
            {
                var existing = context.Permissions.FirstOrDefault(p => p.Id == permission.Id);
                if (existing == null)
                {
                    context.Permissions.Add(permission);
                }
                else
                {
                    existing.Code = permission.Code;
                    existing.Description = permission.Description;
                    existing.Action = permission.Action;
                }
            }
            context.SaveChanges();

            // 3. Seed RolePermissions
            var adminRoleId = Guid.Parse("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98");
            var employeeRoleId = Guid.Parse("7c1d6d5f-f9d7-44c9-b7e2-5282f3f0ab09");
            var hrManagerRoleId = Guid.Parse("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a");
            var teamLeadRoleId = Guid.Parse("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b");
            var accountsManagerRoleId = Guid.Parse("a1000000-0000-0000-0000-000000000001");
            var managerRoleId = Guid.Parse("a1000000-0000-0000-0000-000000000002");
            var sseRoleId = Guid.Parse("a1000000-0000-0000-0000-000000000003");
            var seRoleId = Guid.Parse("a1000000-0000-0000-0000-000000000004");
            var aseRoleId = Guid.Parse("a1000000-0000-0000-0000-000000000005");
            var bdeRoleId = Guid.Parse("a1000000-0000-0000-0000-000000000006");
            var bdaRoleId = Guid.Parse("a1000000-0000-0000-0000-000000000007");

            var rolePermissions = new List<RolePermission>
            {
                // ADMIN gets all permissions
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("50000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("50000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000003") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("70000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("70000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("90000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("90000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("91000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = adminRoleId, PermissionId = Guid.Parse("91000000-0000-0000-0000-000000000002") },

                // EMPLOYEE gets leave.read, leave.write
                new RolePermission { RoleId = employeeRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = employeeRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000002") },

                // HR_MANAGER gets all HR & Leave permissions
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("50000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("50000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = hrManagerRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000003") },

                // TEAM_LEAD gets leave.read, leave.write, leave.approve
                new RolePermission { RoleId = teamLeadRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = teamLeadRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = teamLeadRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000003") },

                // ACCOUNTS gets leave.read/write/approve, invoices.read/write, leads.read
                new RolePermission { RoleId = accountsManagerRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = accountsManagerRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = accountsManagerRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000003") },
                new RolePermission { RoleId = accountsManagerRoleId, PermissionId = Guid.Parse("90000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = accountsManagerRoleId, PermissionId = Guid.Parse("90000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = accountsManagerRoleId, PermissionId = Guid.Parse("70000000-0000-0000-0000-000000000001") },

                // MANAGER gets leave.read, leave.write, leave.approve, leads.read
                new RolePermission { RoleId = managerRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = managerRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = managerRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000003") },
                new RolePermission { RoleId = managerRoleId, PermissionId = Guid.Parse("70000000-0000-0000-0000-000000000001") },

                // SSE gets leave.read, leave.write
                new RolePermission { RoleId = sseRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = sseRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000002") },

                // SE gets leave.read, leave.write
                new RolePermission { RoleId = seRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = seRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000002") },

                // ASE gets leave.read, leave.write
                new RolePermission { RoleId = aseRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = aseRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000002") },

                // BDE gets leave.read/write, leads.read/write
                new RolePermission { RoleId = bdeRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = bdeRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = bdeRoleId, PermissionId = Guid.Parse("70000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = bdeRoleId, PermissionId = Guid.Parse("70000000-0000-0000-0000-000000000002") },

                // BDA gets leave.read/write, leads.read/write
                new RolePermission { RoleId = bdaRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = bdaRoleId, PermissionId = Guid.Parse("60000000-0000-0000-0000-000000000002") },
                new RolePermission { RoleId = bdaRoleId, PermissionId = Guid.Parse("70000000-0000-0000-0000-000000000001") },
                new RolePermission { RoleId = bdaRoleId, PermissionId = Guid.Parse("70000000-0000-0000-0000-000000000002") }
            };

            foreach (var rp in rolePermissions)
            {
                if (!context.RolePermissions.Any(x => x.RoleId == rp.RoleId && x.PermissionId == rp.PermissionId))
                {
                    context.RolePermissions.Add(rp);
                }
            }
            context.SaveChanges();
        }
    }
}
