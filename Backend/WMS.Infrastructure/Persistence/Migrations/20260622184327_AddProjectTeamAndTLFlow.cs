using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectTeamAndTLFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_TeamId",
                table: "Projects",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Teams_TeamId",
                table: "Projects",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // --- Demo seed: one Tech Lead with a team in the IT department, so the
            // create-project Team Lead dropdown is populated on a fresh clone. ---
            var seed = new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var teamId = new Guid("c0000000-0000-0000-0000-000000000001");
            var tlUserId = new Guid("70000000-0000-0000-0000-000000000003");
            var itDeptId = new Guid("50000000-0000-0000-0000-000000000001");
            var techLeadDesigId = new Guid("60000000-0000-0000-0000-000000000004");
            var teamLeadRoleId = new Guid("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b");

            // 1. Team (TeamLeadId null for now to avoid the User↔Team FK cycle).
            migrationBuilder.InsertData(
                table: "Teams",
                columns: new[] { "Id", "Name", "DepartmentId", "TeamLeadId", "CreatedAt", "CreatedBy", "ModifiedAt", "ModifiedBy", "IsDeleted", "DeletedAt", "DeletedBy" },
                values: new object[] { teamId, "Aarav Mehta's Team", itDeptId, null, seed, "System", null, null, false, null, null });

            // 2. Tech Lead user, linked to the team.
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "DesignationId", "Email", "EmployeeCode", "FirstName", "IsActive", "IsFirstTimeLogin", "LastName", "ModifiedAt", "ModifiedBy", "PasswordHash", "PhoneNumber", "TeamId", "Username" },
                values: new object[] { tlUserId, seed, "System", null, null, itDeptId, techLeadDesigId, "aarav.techlead@wms.com", "EMP-TL01", "Aarav", true, false, "Mehta", null, null, "$2a$11$fxpkGie8ewg0L3TFvIEOY.Zh6YHwpqlOcScYRjkZKjPGp/lFCbiZW", "9000000003", teamId, "aarav.techlead" });

            // 3. Point the team back at its lead.
            migrationBuilder.UpdateData(
                table: "Teams",
                keyColumn: "Id",
                keyValue: teamId,
                column: "TeamLeadId",
                value: tlUserId);

            // 4. Give the user the TEAM_LEAD role.
            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[] { tlUserId, teamLeadRoleId });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var teamId = new Guid("c0000000-0000-0000-0000-000000000001");
            var tlUserId = new Guid("70000000-0000-0000-0000-000000000003");
            var teamLeadRoleId = new Guid("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b");

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumns: new[] { "UserId", "RoleId" },
                keyValues: new object[] { tlUserId, teamLeadRoleId });

            // Break the User↔Team cycle before deleting either row.
            migrationBuilder.UpdateData(table: "Teams", keyColumn: "Id", keyValue: teamId, column: "TeamLeadId", value: null);
            migrationBuilder.UpdateData(table: "Users", keyColumn: "Id", keyValue: tlUserId, column: "TeamId", value: null);

            migrationBuilder.DeleteData(table: "Users", keyColumn: "Id", keyValue: tlUserId);
            migrationBuilder.DeleteData(table: "Teams", keyColumn: "Id", keyValue: teamId);

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Teams_TeamId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_TeamId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Projects");
        }
    }
}
