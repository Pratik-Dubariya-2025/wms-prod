using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeProfileAndLeaveRequestDbSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IsActive", "ModifiedAt", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("a7b8c9d0-e1f2-4a3b-b000-5c4d5e6f7a8b"), "LEAVE_MGMT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Manage employee leave requests", 7, true, null, null, "Leave Management" },
                    { new Guid("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8b"), "HR_MGMT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Manage employee profile and salaries", 6, true, null, null, "HR Management" }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "ModifiedAt", "ModifiedBy", "ModuleId", "Scope" },
                values: new object[,]
                {
                    { new Guid("50000000-0000-0000-0000-000000000001"), "Read", "salary.read", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Read employee salary and HR details", null, null, new Guid("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8b"), "all" },
                    { new Guid("50000000-0000-0000-0000-000000000002"), "Write", "salary.write", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Modify employee salary and HR details", null, null, new Guid("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8b"), "all" },
                    { new Guid("60000000-0000-0000-0000-000000000001"), "Read", "leave.read", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Read leave requests", null, null, new Guid("a7b8c9d0-e1f2-4a3b-b000-5c4d5e6f7a8b"), "all" },
                    { new Guid("60000000-0000-0000-0000-000000000002"), "Write", "leave.write", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Submit leave requests", null, null, new Guid("a7b8c9d0-e1f2-4a3b-b000-5c4d5e6f7a8b"), "all" },
                    { new Guid("60000000-0000-0000-0000-000000000003"), "Approve", "leave.approve", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Approve/Reject leave requests", null, null, new Guid("a7b8c9d0-e1f2-4a3b-b000-5c4d5e6f7a8b"), "all" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("60000000-0000-0000-0000-000000000001"), new Guid("7c1d6d5f-f9d7-44c9-b7e2-5282f3f0ab09") },
                    { new Guid("60000000-0000-0000-0000-000000000002"), new Guid("7c1d6d5f-f9d7-44c9-b7e2-5282f3f0ab09") },
                    { new Guid("50000000-0000-0000-0000-000000000001"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") },
                    { new Guid("50000000-0000-0000-0000-000000000002"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") },
                    { new Guid("60000000-0000-0000-0000-000000000001"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") },
                    { new Guid("60000000-0000-0000-0000-000000000002"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") },
                    { new Guid("60000000-0000-0000-0000-000000000003"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") },
                    { new Guid("50000000-0000-0000-0000-000000000001"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") },
                    { new Guid("50000000-0000-0000-0000-000000000002"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") },
                    { new Guid("60000000-0000-0000-0000-000000000001"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") },
                    { new Guid("60000000-0000-0000-0000-000000000002"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") },
                    { new Guid("60000000-0000-0000-0000-000000000003"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") },
                    { new Guid("60000000-0000-0000-0000-000000000001"), new Guid("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b") },
                    { new Guid("60000000-0000-0000-0000-000000000003"), new Guid("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("60000000-0000-0000-0000-000000000001"), new Guid("7c1d6d5f-f9d7-44c9-b7e2-5282f3f0ab09") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("60000000-0000-0000-0000-000000000002"), new Guid("7c1d6d5f-f9d7-44c9-b7e2-5282f3f0ab09") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("50000000-0000-0000-0000-000000000001"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("50000000-0000-0000-0000-000000000002"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("60000000-0000-0000-0000-000000000001"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("60000000-0000-0000-0000-000000000002"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("60000000-0000-0000-0000-000000000003"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("50000000-0000-0000-0000-000000000001"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("50000000-0000-0000-0000-000000000002"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("60000000-0000-0000-0000-000000000001"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("60000000-0000-0000-0000-000000000002"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("60000000-0000-0000-0000-000000000003"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("60000000-0000-0000-0000-000000000001"), new Guid("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b") });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { new Guid("60000000-0000-0000-0000-000000000003"), new Guid("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b") });

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: new Guid("a7b8c9d0-e1f2-4a3b-b000-5c4d5e6f7a8b"));

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: new Guid("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8b"));
        }
    }
}
