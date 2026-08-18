using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedCrmAndInvoiceModulesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IsActive", "ModifiedAt", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8c"), "CRM", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Manage client leads and pipeline stages", 8, true, null, null, "CRM" },
                    { new Guid("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8d"), "ACCOUNTS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Manage financial invoices", 9, true, null, null, "Accounts" }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "ModifiedAt", "ModifiedBy", "ModuleId", "Scope" },
                values: new object[,]
                {
                    { new Guid("70000000-0000-0000-0000-000000000001"), "Read", "leads.read", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Read CRM leads", null, null, new Guid("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8c"), "all" },
                    { new Guid("70000000-0000-0000-0000-000000000002"), "Write", "leads.write", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Modify CRM leads", null, null, new Guid("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8c"), "all" },
                    { new Guid("90000000-0000-0000-0000-000000000001"), "Read", "invoices.read", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Read invoices", null, null, new Guid("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8d"), "all" },
                    { new Guid("90000000-0000-0000-0000-000000000002"), "Write", "invoices.write", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Modify invoices", null, null, new Guid("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8d"), "all" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: new Guid("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8c"));

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: new Guid("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8d"));
        }
    }
}
