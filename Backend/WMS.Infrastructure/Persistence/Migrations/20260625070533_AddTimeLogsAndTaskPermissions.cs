using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeLogsAndTaskPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TimeLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoggedHours = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    LogDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ApprovedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeLogs_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeLogs_Users_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TimeLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeLogs_ApprovedById",
                table: "TimeLogs",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_TimeLogs_TaskId",
                table: "TimeLogs",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeLogs_UserId",
                table: "TimeLogs",
                column: "UserId");

            // Seed TimeLog Permissions
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var taskModuleId = new Guid("d4e5f6a7-b8c9-4d5e-a000-2b3c4d5e6f7a");

            var permCreateId = new Guid("c0000000-0000-0000-0000-000000000001");
            var permReadId = new Guid("c0000000-0000-0000-0000-000000000002");
            var permApproveId = new Guid("c0000000-0000-0000-0000-000000000003");

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "ModifiedAt", "ModifiedBy", "ModuleId", "Scope" },
                values: new object[,]
                {
                    { permCreateId, "Create", "timelog.create", seedDate, null, null, null, "Log time against tasks", null, null, taskModuleId, "self" },
                    { permReadId, "Read", "timelog.read", seedDate, null, null, null, "Read task time logs", null, null, taskModuleId, "all" },
                    { permApproveId, "Approve", "timelog.approve", seedDate, null, null, null, "Approve time logs", null, null, taskModuleId, "all" }
                });

            // Role IDs
            var adminRoleId = Guid.Parse("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98");
            var hrManagerRoleId = Guid.Parse("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a");
            var accountsManagerRoleId = Guid.Parse("a1000000-0000-0000-0000-000000000001");
            var managerRoleId = Guid.Parse("a1000000-0000-0000-0000-000000000002");
            var bdeRoleId = Guid.Parse("a1000000-0000-0000-0000-000000000006");
            var teamLeadRoleId = Guid.Parse("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b");
            var bdaRoleId = Guid.Parse("a1000000-0000-0000-0000-000000000007");
            var sseRoleId = Guid.Parse("a1000000-0000-0000-0000-000000000003");
            var seRoleId = Guid.Parse("a1000000-0000-0000-0000-000000000004");
            var aseRoleId = Guid.Parse("a1000000-0000-0000-0000-000000000005");
            var employeeRoleId = Guid.Parse("7c1d6d5f-f9d7-44c9-b7e2-5282f3f0ab09");

            var rolesForCreate = new[] { adminRoleId, hrManagerRoleId, accountsManagerRoleId, managerRoleId, bdeRoleId, teamLeadRoleId, bdaRoleId, sseRoleId, seRoleId, aseRoleId, employeeRoleId };
            var rolesForRead = new[] { adminRoleId, hrManagerRoleId, accountsManagerRoleId, managerRoleId, teamLeadRoleId, sseRoleId, seRoleId, aseRoleId, employeeRoleId };
            var rolesForApprove = new[] { adminRoleId, managerRoleId, teamLeadRoleId };

            // Grant timelog.create
            foreach (var roleId in rolesForCreate)
            {
                migrationBuilder.InsertData(
                    table: "RolePermissions",
                    columns: new[] { "PermissionId", "RoleId" },
                    values: new object[] { permCreateId, roleId });
            }

            // Grant timelog.read
            foreach (var roleId in rolesForRead)
            {
                migrationBuilder.InsertData(
                    table: "RolePermissions",
                    columns: new[] { "PermissionId", "RoleId" },
                    values: new object[] { permReadId, roleId });
            }

            // Grant timelog.approve
            foreach (var roleId in rolesForApprove)
            {
                migrationBuilder.InsertData(
                    table: "RolePermissions",
                    columns: new[] { "PermissionId", "RoleId" },
                    values: new object[] { permApproveId, roleId });
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var permCreateId = new Guid("c0000000-0000-0000-0000-000000000001");
            var permReadId = new Guid("c0000000-0000-0000-0000-000000000002");
            var permApproveId = new Guid("c0000000-0000-0000-0000-000000000003");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: permCreateId);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: permReadId);

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: permApproveId);

            migrationBuilder.DropTable(
                name: "TimeLogs");
        }
    }
}
