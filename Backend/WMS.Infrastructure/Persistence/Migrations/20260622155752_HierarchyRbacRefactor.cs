using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HierarchyRbacRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Roles",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Roles",
                type: "int",
                nullable: false,
                defaultValue: 100);

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
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("7c1d6d5f-f9d7-44c9-b7e2-5282f3f0ab09"),
                columns: new[] { "Code", "Priority" },
                values: new object[] { "EMPLOYEE", 100 });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98"),
                columns: new[] { "Code", "Description", "Priority" },
                values: new object[] { "ADMIN", "Full system access, user management, global configuration", 1 });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a"),
                columns: new[] { "Code", "Description", "Priority" },
                values: new object[] { "HR", "All employee PII, salary, leave, payroll across all departments", 10 });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b"),
                columns: new[] { "Code", "Description", "IsSystemRole", "Priority" },
                values: new object[] { "TL", "Their team's tasks, timesheets, performance reviews", true, 30 });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsSystemRole", "ModifiedAt", "ModifiedBy", "Name", "Priority" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000000001"), "ACCOUNTS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Invoices, expenses, payroll, financial reports", true, null, null, "ACCOUNTS_MANAGER", 10 },
                    { new Guid("a1000000-0000-0000-0000-000000000002"), "MANAGER", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "All teams in their department, performance, approvals", true, null, null, "DEPARTMENT_MANAGER", 20 },
                    { new Guid("a1000000-0000-0000-0000-000000000003"), "SSE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Own work plus can review junior engineer PRs", true, null, null, "SENIOR_SOFTWARE_ENGINEER", 40 },
                    { new Guid("a1000000-0000-0000-0000-000000000004"), "SE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Own tasks and timesheets only", true, null, null, "SOFTWARE_ENGINEER", 50 },
                    { new Guid("a1000000-0000-0000-0000-000000000005"), "ASE_TSE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Own tasks only, read-only project visibility", true, null, null, "ASSOCIATE_SOFTWARE_ENGINEER", 60 },
                    { new Guid("a1000000-0000-0000-0000-000000000006"), "BDE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Lead pipeline for their region", true, null, null, "BUSINESS_DEV_EXECUTIVE", 25 },
                    { new Guid("a1000000-0000-0000-0000-000000000007"), "BDA", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Own leads only, limited CRM access", true, null, null, "BUSINESS_DEV_ASSOCIATE", 35 }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000001"),
                columns: new[] { "ManagerId", "TeamId" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000002"),
                columns: new[] { "ManagerId", "TeamId" },
                values: new object[] { null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ManagerId",
                table: "Users",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TeamId",
                table: "Users",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Code",
                table: "Roles",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Teams_TeamId",
                table: "Users",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_ManagerId",
                table: "Users",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Rewrite sp_InviteUser to rank role authority by Role.Priority (lower = higher
            // authority) instead of permission count, and set the invitee's ManagerId to the
            // inviter (reports-to hierarchy).
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE sp_InviteUser
    @EmployeeCode VARCHAR(20),
    @FirstName VARCHAR(50),
    @LastName VARCHAR(50),
    @Email VARCHAR(100),
    @Username VARCHAR(50),
    @PasswordHash VARCHAR(256),
    @PhoneNumber VARCHAR(20),
    @DepartmentId UNIQUEIDENTIFIER,
    @DesignationId UNIQUEIDENTIFIER,
    @CreatedBy VARCHAR(100),
    @RoleIdsJson NVARCHAR(MAX),
    @CurrentUserId UNIQUEIDENTIFIER,
    @NewUserId UNIQUEIDENTIFIER OUTPUT,
    @ErrorCode INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @ErrorCode = 0;

    IF EXISTS (SELECT 1 FROM Users WHERE LOWER(Email) = LOWER(@Email) AND IsDeleted = 0)
    BEGIN SET @ErrorCode = 1; RETURN; END

    IF EXISTS (SELECT 1 FROM Users WHERE LOWER(Username) = LOWER(@Username) AND IsDeleted = 0)
    BEGIN SET @ErrorCode = 2; RETURN; END

    IF EXISTS (SELECT 1 FROM Users WHERE LOWER(EmployeeCode) = LOWER(@EmployeeCode) AND IsDeleted = 0)
    BEGIN SET @ErrorCode = 3; RETURN; END

    IF NOT EXISTS (SELECT 1 FROM Departments WHERE Id = @DepartmentId AND IsDeleted = 0)
    BEGIN SET @ErrorCode = 4; RETURN; END

    DECLARE @InvitedDesignationLevel INT = 0;
    DECLARE @InvitedDesignationName VARCHAR(100);
    SELECT @InvitedDesignationLevel = ISNULL(Level, 0), @InvitedDesignationName = Name
    FROM Designations WHERE Id = @DesignationId AND IsDeleted = 0;

    IF @InvitedDesignationName IS NULL
    BEGIN SET @ErrorCode = 5; RETURN; END

    DECLARE @InvitedTopPriority INT = 2147483647;
    DECLARE @PassedRoleCount INT = 0;
    DECLARE @ValidRoleCount INT = 0;

    SELECT @PassedRoleCount = COUNT(*) FROM OPENJSON(@RoleIdsJson);

    SELECT @ValidRoleCount = COUNT(*),
           @InvitedTopPriority = ISNULL(MIN(r.Priority), 2147483647)
    FROM Roles r
    WHERE r.IsDeleted = 0
      AND r.Id IN (SELECT CAST(value AS UNIQUEIDENTIFIER) FROM OPENJSON(@RoleIdsJson));

    IF @ValidRoleCount <> @PassedRoleCount OR @PassedRoleCount = 0
    BEGIN SET @ErrorCode = 7; RETURN; END

    DECLARE @CurrentUserDesignationLevel INT = 0;
    DECLARE @CurrentUserTopPriority INT = 2147483647;
    DECLARE @IsAdmin BIT = 0;

    IF EXISTS (
        SELECT 1 FROM UserRoles ur
        JOIN Roles r ON ur.RoleId = r.Id
        WHERE ur.UserId = @CurrentUserId AND r.Code = 'ADMIN')
    BEGIN SET @IsAdmin = 1; END

    SELECT @CurrentUserDesignationLevel = ISNULL(d.Level, 0)
    FROM Users u LEFT JOIN Designations d ON u.DesignationId = d.Id
    WHERE u.Id = @CurrentUserId AND u.IsDeleted = 0;

    SELECT @CurrentUserTopPriority = ISNULL(MIN(r.Priority), 2147483647)
    FROM UserRoles ur JOIN Roles r ON ur.RoleId = r.Id
    WHERE ur.UserId = @CurrentUserId AND r.IsDeleted = 0;

    IF @IsAdmin = 0 AND @InvitedDesignationLevel >= @CurrentUserDesignationLevel
    BEGIN SET @ErrorCode = 6; RETURN; END

    -- Lower Priority = higher authority. Invitee must be strictly lower authority.
    IF @IsAdmin = 0 AND @InvitedTopPriority <= @CurrentUserTopPriority
    BEGIN SET @ErrorCode = 8; RETURN; END

    BEGIN TRANSACTION;
    BEGIN TRY
        SET @NewUserId = NEWID();

        INSERT INTO Users (Id, EmployeeCode, FirstName, LastName, Email, Username, PasswordHash, PhoneNumber, DepartmentId, DesignationId, ManagerId, IsActive, IsFirstTimeLogin, IsDeleted, CreatedBy, CreatedAt)
        VALUES (@NewUserId, @EmployeeCode, @FirstName, @LastName, @Email, @Username, @PasswordHash, @PhoneNumber, @DepartmentId, @DesignationId, @CurrentUserId, 1, 1, 0, @CreatedBy, GETUTCDATE());

        IF @RoleIdsJson IS NOT NULL AND ISJSON(@RoleIdsJson) > 0
        BEGIN
            INSERT INTO UserRoles (UserId, RoleId)
            SELECT @NewUserId, CAST(value AS UNIQUEIDENTIFIER) FROM OPENJSON(@RoleIdsJson);
        END

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SET @ErrorCode = 99;
        THROW;
    END CATCH
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the previous permission-count based sp_InviteUser (pre-priority).
            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE sp_InviteUser
    @EmployeeCode VARCHAR(20),
    @FirstName VARCHAR(50),
    @LastName VARCHAR(50),
    @Email VARCHAR(100),
    @Username VARCHAR(50),
    @PasswordHash VARCHAR(256),
    @PhoneNumber VARCHAR(20),
    @DepartmentId UNIQUEIDENTIFIER,
    @DesignationId UNIQUEIDENTIFIER,
    @CreatedBy VARCHAR(100),
    @RoleIdsJson NVARCHAR(MAX),
    @CurrentUserId UNIQUEIDENTIFIER,
    @NewUserId UNIQUEIDENTIFIER OUTPUT,
    @ErrorCode INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @ErrorCode = 0;

    IF EXISTS (SELECT 1 FROM Users WHERE LOWER(Email) = LOWER(@Email) AND IsDeleted = 0)
    BEGIN SET @ErrorCode = 1; RETURN; END
    IF EXISTS (SELECT 1 FROM Users WHERE LOWER(Username) = LOWER(@Username) AND IsDeleted = 0)
    BEGIN SET @ErrorCode = 2; RETURN; END
    IF EXISTS (SELECT 1 FROM Users WHERE LOWER(EmployeeCode) = LOWER(@EmployeeCode) AND IsDeleted = 0)
    BEGIN SET @ErrorCode = 3; RETURN; END
    IF NOT EXISTS (SELECT 1 FROM Departments WHERE Id = @DepartmentId AND IsDeleted = 0)
    BEGIN SET @ErrorCode = 4; RETURN; END

    DECLARE @InvitedDesignationLevel INT = 0;
    DECLARE @InvitedDesignationName VARCHAR(100);
    SELECT @InvitedDesignationLevel = ISNULL(Level, 0), @InvitedDesignationName = Name
    FROM Designations WHERE Id = @DesignationId AND IsDeleted = 0;
    IF @InvitedDesignationName IS NULL
    BEGIN SET @ErrorCode = 5; RETURN; END

    DECLARE @InvitedMaxRoleRank INT = 0;
    DECLARE @PassedRoleCount INT = 0;
    DECLARE @ValidRoleCount INT = 0;
    SELECT @PassedRoleCount = COUNT(*) FROM OPENJSON(@RoleIdsJson);
    SELECT @ValidRoleCount = COUNT(*), @InvitedMaxRoleRank = ISNULL(MAX(p.PermCount), 0)
    FROM Roles r
    OUTER APPLY (SELECT COUNT(*) AS PermCount FROM RolePermissions rp WHERE rp.RoleId = r.Id) p
    WHERE r.Id IN (SELECT CAST(value AS UNIQUEIDENTIFIER) FROM OPENJSON(@RoleIdsJson));
    IF @ValidRoleCount <> @PassedRoleCount OR @PassedRoleCount = 0
    BEGIN SET @ErrorCode = 7; RETURN; END

    DECLARE @CurrentUserDesignationLevel INT = 0;
    DECLARE @CurrentUserMaxRoleRank INT = 0;
    DECLARE @IsAdmin BIT = 0;
    IF EXISTS (SELECT 1 FROM UserRoles ur JOIN Roles r ON ur.RoleId = r.Id WHERE ur.UserId = @CurrentUserId AND r.Name = 'ADMIN')
    BEGIN SET @IsAdmin = 1; END
    SELECT @CurrentUserDesignationLevel = ISNULL(d.Level, 0)
    FROM Users u LEFT JOIN Designations d ON u.DesignationId = d.Id
    WHERE u.Id = @CurrentUserId AND u.IsDeleted = 0;
    SELECT @CurrentUserMaxRoleRank = ISNULL(MAX(p.PermCount), 0)
    FROM UserRoles ur
    OUTER APPLY (SELECT COUNT(*) AS PermCount FROM RolePermissions rp WHERE rp.RoleId = ur.RoleId) p
    WHERE ur.UserId = @CurrentUserId;

    IF @IsAdmin = 0 AND @InvitedDesignationLevel >= @CurrentUserDesignationLevel
    BEGIN SET @ErrorCode = 6; RETURN; END
    IF @IsAdmin = 0 AND @InvitedMaxRoleRank >= @CurrentUserMaxRoleRank
    BEGIN SET @ErrorCode = 8; RETURN; END

    BEGIN TRANSACTION;
    BEGIN TRY
        SET @NewUserId = NEWID();
        INSERT INTO Users (Id, EmployeeCode, FirstName, LastName, Email, Username, PasswordHash, PhoneNumber, DepartmentId, DesignationId, IsActive, IsFirstTimeLogin, IsDeleted, CreatedBy, CreatedAt)
        VALUES (@NewUserId, @EmployeeCode, @FirstName, @LastName, @Email, @Username, @PasswordHash, @PhoneNumber, @DepartmentId, @DesignationId, 1, 1, 0, @CreatedBy, GETUTCDATE());
        IF @RoleIdsJson IS NOT NULL AND ISJSON(@RoleIdsJson) > 0
        BEGIN
            INSERT INTO UserRoles (UserId, RoleId)
            SELECT @NewUserId, CAST(value AS UNIQUEIDENTIFIER) FROM OPENJSON(@RoleIdsJson);
        END
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SET @ErrorCode = 99;
        THROW;
    END CATCH
END
");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Teams_TeamId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_ManagerId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ManagerId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_TeamId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Roles_Code",
                table: "Roles");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1000000-0000-0000-0000-000000000007"));

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "Permissions");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98"),
                column: "Description",
                value: "Full system access");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a"),
                column: "Description",
                value: "HR department management access");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b"),
                columns: new[] { "Description", "IsSystemRole" },
                values: new object[] { "Team lead access with approval rights", false });
        }
    }
}
