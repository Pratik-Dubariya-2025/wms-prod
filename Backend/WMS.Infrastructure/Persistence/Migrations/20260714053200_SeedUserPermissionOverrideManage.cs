using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedUserPermissionOverrideManage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [Permissions] WHERE [Id] = '30000000-0000-0000-0000-000000000005')
INSERT INTO [Permissions] ([Id], [Action], [Code], [CreatedAt], [CreatedBy], [DeletedAt], [DeletedBy], [Description], [ModifiedAt], [ModifiedBy], [ModuleId])
VALUES ('30000000-0000-0000-0000-000000000005', 'ManagePermissions', 'permission.override.manage', '2026-01-01T00:00:00.0000000Z', NULL, NULL, NULL, 'Grant/revoke a specific user''s permission overrides, independent of role-definition rights so it can be delegated and scoped via PBAC', NULL, NULL, 'a1b2c3d4-e5f6-4a5b-8c7d-9e0f1a2b3c4d');
");

            // permission.override.manage used to be gated by role.update alone (same code did both
            // "edit role definitions" and "grant/revoke one user's permission overrides"). Grant it to
            // every role that currently holds role.update so no existing workflow regresses; PBAC can
            // then be used to scope it down independently going forward (e.g. to a manager's
            // subordinates + specific modules) without touching role.update's own semantics.
            migrationBuilder.Sql(@"
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT rp.RoleId, '30000000-0000-0000-0000-000000000005'
FROM RolePermissions rp
WHERE rp.PermissionId = '30000000-0000-0000-0000-000000000003';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000005"));
        }
    }
}
