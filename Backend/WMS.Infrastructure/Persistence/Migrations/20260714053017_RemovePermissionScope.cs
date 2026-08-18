using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovePermissionScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Scope",
                table: "Permissions");

            // POLICY_MGMT module + policy.read/policy.manage were already declared in
            // PermissionConfiguration's HasData but had no migration scaffolded for them yet
            // (pre-existing drift, unrelated to the Scope removal above - some environments already
            // have this data via DbInitializer's separate runtime seeding, others don't). Guarded so
            // it's a no-op where the rows already exist and a real seed on a fresh database.
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [Modules] WHERE [Id] = 'f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8e')
INSERT INTO [Modules] ([Id], [Code], [CreatedAt], [CreatedBy], [DeletedAt], [DeletedBy], [Description], [DisplayOrder], [IsActive], [ModifiedAt], [ModifiedBy], [Name])
VALUES ('f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8e', 'POLICY_MGMT', '2026-01-01T00:00:00.0000000Z', NULL, NULL, NULL, 'Manage fine-grained access policies', 10, CAST(1 AS bit), NULL, NULL, 'Policy Management');

IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Id] = '91000000-0000-0000-0000-000000000001')
INSERT INTO [Permissions] ([Id], [Action], [Code], [CreatedAt], [CreatedBy], [DeletedAt], [DeletedBy], [Description], [ModifiedAt], [ModifiedBy], [ModuleId])
VALUES ('91000000-0000-0000-0000-000000000001', 'Read', 'policy.read', '2026-01-01T00:00:00.0000000Z', NULL, NULL, NULL, 'Read access policies', NULL, NULL, 'f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8e');

IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Id] = '91000000-0000-0000-0000-000000000002')
INSERT INTO [Permissions] ([Id], [Action], [Code], [CreatedAt], [CreatedBy], [DeletedAt], [DeletedBy], [Description], [ModifiedAt], [ModifiedBy], [ModuleId])
VALUES ('91000000-0000-0000-0000-000000000002', 'Write', 'policy.manage', '2026-01-01T00:00:00.0000000Z', NULL, NULL, NULL, 'Modify access policies', NULL, NULL, 'f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8e');
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("91000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("91000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValue: new Guid("f6a7b8c9-d0e1-4f2a-b000-4c4d5e6f7a8e"));

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "Permissions",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                defaultValue: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000002"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000003"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000004"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000001"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000002"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000003"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000001"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000002"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000001"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000002"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000003"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000004"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000001"),
                column: "Scope",
                value: "all");

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("90000000-0000-0000-0000-000000000002"),
                column: "Scope",
                value: "all");
        }
    }
}
