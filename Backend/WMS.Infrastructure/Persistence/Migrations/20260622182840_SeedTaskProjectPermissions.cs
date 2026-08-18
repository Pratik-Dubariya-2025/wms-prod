using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WMS.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// The Task Management / Project Management modules and their task.* / project.*
    /// permissions were declared in EF HasData (and the model snapshot) but no earlier
    /// migration actually inserted them, so the database was missing them. This migration
    /// inserts them explicitly and grants them to the ADMIN role.
    /// </summary>
    public partial class SeedTaskProjectPermissions : Migration
    {
        private static readonly DateTime Seed = new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        private static readonly Guid TaskModuleId = new Guid("d4e5f6a7-b8c9-4d5e-a000-2b3c4d5e6f7a");
        private static readonly Guid ProjectModuleId = new Guid("e5f6a7b8-c9d0-4e5f-b000-3c4d5e6f7a8b");
        private static readonly Guid AdminRoleId = new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98");

        private static readonly Guid[] PermissionIds =
        {
            new Guid("40000000-0000-0000-0000-000000000001"), // task.create
            new Guid("40000000-0000-0000-0000-000000000002"), // task.read
            new Guid("40000000-0000-0000-0000-000000000003"), // task.update
            new Guid("40000000-0000-0000-0000-000000000004"), // task.delete
            new Guid("80000000-0000-0000-0000-000000000001"), // project.create
            new Guid("80000000-0000-0000-0000-000000000002"), // project.read
            new Guid("80000000-0000-0000-0000-000000000003"), // project.update
            new Guid("80000000-0000-0000-0000-000000000004"), // project.delete
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Modules (insert only if missing — these were never seeded by a migration).
            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IsActive", "ModifiedAt", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { TaskModuleId, "TASK_MGMT", Seed, null, null, null, "Manage tasks and status transitions", 4, true, null, null, "Task Management" },
                    { ProjectModuleId, "PROJECT_MGMT", Seed, null, null, null, "Manage client projects and departments", 5, true, null, null, "Project Management" }
                });

            // 2. Permissions (resource.action) with default scope 'all'.
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "ModifiedAt", "ModifiedBy", "ModuleId", "Scope" },
                values: new object[,]
                {
                    { PermissionIds[0], "Create", "task.create", Seed, null, null, null, "Create tasks", null, null, TaskModuleId, "all" },
                    { PermissionIds[1], "Read", "task.read", Seed, null, null, null, "Read tasks", null, null, TaskModuleId, "all" },
                    { PermissionIds[2], "Update", "task.update", Seed, null, null, null, "Update tasks", null, null, TaskModuleId, "all" },
                    { PermissionIds[3], "Delete", "task.delete", Seed, null, null, null, "Delete tasks", null, null, TaskModuleId, "all" },
                    { PermissionIds[4], "Create", "project.create", Seed, null, null, null, "Create projects", null, null, ProjectModuleId, "all" },
                    { PermissionIds[5], "Read", "project.read", Seed, null, null, null, "Read projects", null, null, ProjectModuleId, "all" },
                    { PermissionIds[6], "Update", "project.update", Seed, null, null, null, "Update projects", null, null, ProjectModuleId, "all" },
                    { PermissionIds[7], "Delete", "project.delete", Seed, null, null, null, "Delete projects", null, null, ProjectModuleId, "all" }
                });

            // 3. Grant all of them to ADMIN.
            foreach (var pid in PermissionIds)
            {
                migrationBuilder.InsertData(
                    table: "RolePermissions",
                    columns: new[] { "PermissionId", "RoleId" },
                    values: new object[] { pid, AdminRoleId });
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var pid in PermissionIds)
            {
                migrationBuilder.DeleteData(
                    table: "RolePermissions",
                    keyColumns: new[] { "RoleId", "PermissionId" },
                    keyValues: new object[] { AdminRoleId, pid });
            }

            foreach (var pid in PermissionIds)
            {
                migrationBuilder.DeleteData(
                    table: "Permissions",
                    keyColumn: "Id",
                    keyValue: pid);
            }

            migrationBuilder.DeleteData(table: "Modules", keyColumn: "Id", keyValue: TaskModuleId);
            migrationBuilder.DeleteData(table: "Modules", keyColumn: "Id", keyValue: ProjectModuleId);
        }
    }
}
