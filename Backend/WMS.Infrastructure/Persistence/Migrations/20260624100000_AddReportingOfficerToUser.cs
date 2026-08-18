using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReportingOfficerToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReportingOfficerId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ReportingOfficerId",
                table: "Users",
                column: "ReportingOfficerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_ReportingOfficerId",
                table: "Users",
                column: "ReportingOfficerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
CREATE OR ALTER PROCEDURE sp_InviteUser
    @EmployeeCode          VARCHAR(20),
    @FirstName             VARCHAR(50),
    @LastName              VARCHAR(50),
    @Email                 VARCHAR(100),
    @Username              VARCHAR(50),
    @PasswordHash          VARCHAR(256),
    @PhoneNumber           VARCHAR(20),
    @DepartmentId          UNIQUEIDENTIFIER,
    @DesignationId         UNIQUEIDENTIFIER,
    @CreatedBy             VARCHAR(100),
    @RoleIdsJson           NVARCHAR(MAX),
    @CurrentUserId         UNIQUEIDENTIFIER,
    @ManagerId             UNIQUEIDENTIFIER,           -- REQUIRED, replaces CurrentUserId auto-assign
    @ReportingOfficerId    UNIQUEIDENTIFIER = NULL,    -- OPTIONAL
    @NewUserId             UNIQUEIDENTIFIER OUTPUT,
    @ErrorCode             INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @ErrorCode = 0;

    -- 1. Duplicate email
    IF EXISTS (SELECT 1 FROM Users WHERE LOWER(Email) = LOWER(@Email) AND IsDeleted = 0)
    BEGIN SET @ErrorCode = 1; RETURN; END

    -- 2. Duplicate username
    IF EXISTS (SELECT 1 FROM Users WHERE LOWER(Username) = LOWER(@Username) AND IsDeleted = 0)
    BEGIN SET @ErrorCode = 2; RETURN; END

    -- 3. Duplicate employee code
    IF EXISTS (SELECT 1 FROM Users WHERE LOWER(EmployeeCode) = LOWER(@EmployeeCode) AND IsDeleted = 0)
    BEGIN SET @ErrorCode = 3; RETURN; END

    -- 4. Validate Department
    IF NOT EXISTS (SELECT 1 FROM Departments WHERE Id = @DepartmentId AND IsDeleted = 0)
    BEGIN SET @ErrorCode = 4; RETURN; END

    -- 5. Validate Designation — load invited level
    DECLARE @InvitedLevel INT;
    SELECT @InvitedLevel = d.Level
    FROM   Designations d
    WHERE  d.Id = @DesignationId AND d.IsDeleted = 0;

    IF @InvitedLevel IS NULL
    BEGIN SET @ErrorCode = 5; RETURN; END

    -- 6. Validate Roles exist and find highest authority (lowest Priority number).
    DECLARE @InvitedTopPriority INT = 2147483647;
    DECLARE @PassedRoleCount INT = 0;
    DECLARE @ValidRoleCount INT = 0;

    SELECT @PassedRoleCount = COUNT(*) FROM OPENJSON(@RoleIdsJson);

    SELECT @ValidRoleCount = COUNT(*),
           @InvitedTopPriority = ISNULL(MIN(r.Priority), 2147483647)
    FROM Roles r
    WHERE r.IsDeleted = 0
      AND r.Id IN (
        SELECT CAST(value AS UNIQUEIDENTIFIER)
        FROM OPENJSON(@RoleIdsJson)
    );

    IF @ValidRoleCount <> @PassedRoleCount OR @PassedRoleCount = 0
    BEGIN
        SET @ErrorCode = 7;
        RETURN;
    END

    -- 7. Get Current User's Designation Level and highest authority role priority.
    DECLARE @CurrentUserDesignationLevel INT = 0;
    DECLARE @CurrentUserTopPriority INT = 2147483647;
    DECLARE @IsAdmin BIT = 0;

    -- Check if Current User has the 'ADMIN' role (by code)
    IF EXISTS (
        SELECT 1
        FROM UserRoles ur
        JOIN Roles r ON ur.RoleId = r.Id
        WHERE ur.UserId = @CurrentUserId AND r.Code = 'ADMIN'
    )
    BEGIN
        SET @IsAdmin = 1;
    END

    SELECT @CurrentUserDesignationLevel = ISNULL(d.Level, 0)
    FROM Users u
    LEFT JOIN Designations d ON u.DesignationId = d.Id
    WHERE u.Id = @CurrentUserId AND u.IsDeleted = 0;

    SELECT @CurrentUserTopPriority = ISNULL(MIN(r.Priority), 2147483647)
    FROM UserRoles ur
    JOIN Roles r ON ur.RoleId = r.Id
    WHERE ur.UserId = @CurrentUserId AND r.IsDeleted = 0;

    -- 8. Check Designation Level Hierarchy (Bypassed for ADMIN)
    IF @IsAdmin = 0 AND @InvitedLevel >= @CurrentUserDesignationLevel
    BEGIN
        SET @ErrorCode = 6;
        RETURN;
    END

    -- 9. Check Role Authority Hierarchy by Priority (Bypassed for ADMIN).
    IF @IsAdmin = 0 AND @InvitedTopPriority <= @CurrentUserTopPriority
    BEGIN
        SET @ErrorCode = 8;
        RETURN;
    END

    -- 9. Validate ManagerId (REQUIRED)
    --    Must exist, be active, be in same dept, and be at the MAX level of that dept.
    DECLARE @MaxLevelInDept INT;
    SELECT @MaxLevelInDept = MAX(d.Level)
    FROM   Users u
    JOIN   Designations d ON d.Id = u.DesignationId
    WHERE  u.DepartmentId = @DepartmentId
      AND  u.IsDeleted = 0
      AND  u.IsActive = 1;

    DECLARE @ManagerLevel INT;
    DECLARE @ManagerDeptId UNIQUEIDENTIFIER;
    SELECT @ManagerLevel  = d.Level,
           @ManagerDeptId = u.DepartmentId
    FROM   Users u
    JOIN   Designations d ON d.Id = u.DesignationId
    WHERE  u.Id = @ManagerId
      AND  u.IsDeleted = 0
      AND  u.IsActive = 1;

    IF @ManagerDeptId IS NULL
        OR @ManagerDeptId <> @DepartmentId
        OR @ManagerLevel <> @MaxLevelInDept   -- must be at highest level in dept
        OR @ManagerLevel <= @InvitedLevel     -- must be above invited designation
    BEGIN SET @ErrorCode = 9; RETURN; END     -- 9 = invalid manager

    -- 10. Validate ReportingOfficerId (OPTIONAL)
    --     If provided: same dept, level strictly above invited, level <= manager level.
    DECLARE @DerivedTeamId UNIQUEIDENTIFIER = NULL;
    DECLARE @ROLevel INT;

    IF @ReportingOfficerId IS NOT NULL
    BEGIN
        DECLARE @RODeptId UNIQUEIDENTIFIER;
        SELECT @ROLevel   = d.Level,
               @RODeptId  = u.DepartmentId,
               @DerivedTeamId = u.TeamId      -- derive TeamId from RO
        FROM   Users u
        JOIN   Designations d ON d.Id = u.DesignationId
        WHERE  u.Id = @ReportingOfficerId
          AND  u.IsDeleted = 0
          AND  u.IsActive = 1;

        IF @RODeptId IS NULL
            OR @RODeptId <> @DepartmentId
            OR @ROLevel <= @InvitedLevel       -- RO must be strictly above invited
            OR @ROLevel > @ManagerLevel        -- RO cannot outrank manager
        BEGIN SET @ErrorCode = 10; RETURN; END -- 10 = invalid reporting officer
    END

    -- Insert user
    BEGIN TRANSACTION;
    BEGIN TRY
        SET @NewUserId = NEWID();

        INSERT INTO Users (
            Id, EmployeeCode, FirstName, LastName, Email, Username,
            PasswordHash, PhoneNumber, DepartmentId, DesignationId,
            ManagerId,              -- org manager (selected, not auto-assigned)
            ReportingOfficerId,     -- NEW — daily officer
            TeamId,                 -- derived from RO inside SP
            IsActive, IsFirstTimeLogin, IsDeleted, CreatedBy, CreatedAt
        )
        VALUES (
            @NewUserId, @EmployeeCode, @FirstName, @LastName, @Email,
            @Username, @PasswordHash, @PhoneNumber, @DepartmentId,
            @DesignationId, @ManagerId, @ReportingOfficerId,
            @DerivedTeamId,         -- null if no RO given or RO has no team
            1, 1, 0, @CreatedBy, GETUTCDATE()
        );

        -- Role assignment (unchanged)
        IF @RoleIdsJson IS NOT NULL AND ISJSON(@RoleIdsJson) > 0
        BEGIN
            INSERT INTO UserRoles (UserId, RoleId)
            SELECT @NewUserId, CAST(value AS UNIQUEIDENTIFIER)
            FROM   OPENJSON(@RoleIdsJson);
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
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_ReportingOfficerId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ReportingOfficerId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ReportingOfficerId",
                table: "Users");

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

    -- 1. Check duplicate email
    IF EXISTS (SELECT 1 FROM Users WHERE LOWER(Email) = LOWER(@Email) AND IsDeleted = 0)
    BEGIN
        SET @ErrorCode = 1;
        RETURN;
    END

    -- 2. Check duplicate username
    IF EXISTS (SELECT 1 FROM Users WHERE LOWER(Username) = LOWER(@Username) AND IsDeleted = 0)
    BEGIN
        SET @ErrorCode = 2;
        RETURN;
    END

    -- 3. Check duplicate employee code
    IF EXISTS (SELECT 1 FROM Users WHERE LOWER(EmployeeCode) = LOWER(@EmployeeCode) AND IsDeleted = 0)
    BEGIN
        SET @ErrorCode = 3;
        RETURN;
    END

    -- 4. Validate Department exists
    IF NOT EXISTS (SELECT 1 FROM Departments WHERE Id = @DepartmentId AND IsDeleted = 0)
    BEGIN
        SET @ErrorCode = 4;
        RETURN;
    END

    -- 5. Validate Designation exists and load level
    DECLARE @InvitedDesignationLevel INT = 0;
    DECLARE @InvitedDesignationName VARCHAR(100);
    SELECT @InvitedDesignationLevel = ISNULL(Level, 0), @InvitedDesignationName = Name
    FROM Designations
    WHERE Id = @DesignationId AND IsDeleted = 0;

    IF @InvitedDesignationName IS NULL
    BEGIN
        SET @ErrorCode = 5;
        RETURN;
    END

    -- 6. Validate Roles exist and find highest authority (lowest Priority number).
    DECLARE @InvitedTopPriority INT = 2147483647;
    DECLARE @PassedRoleCount INT = 0;
    DECLARE @ValidRoleCount INT = 0;

    SELECT @PassedRoleCount = COUNT(*) FROM OPENJSON(@RoleIdsJson);

    SELECT @ValidRoleCount = COUNT(*),
           @InvitedTopPriority = ISNULL(MIN(r.Priority), 2147483647)
    FROM Roles r
    WHERE r.IsDeleted = 0
      AND r.Id IN (
        SELECT CAST(value AS UNIQUEIDENTIFIER)
        FROM OPENJSON(@RoleIdsJson)
    );

    IF @ValidRoleCount <> @PassedRoleCount OR @PassedRoleCount = 0
    BEGIN
        SET @ErrorCode = 7;
        RETURN;
    END

    -- 7. Get Current User's Designation Level and highest authority role priority.
    DECLARE @CurrentUserDesignationLevel INT = 0;
    DECLARE @CurrentUserTopPriority INT = 2147483647;
    DECLARE @IsAdmin BIT = 0;

    -- Check if Current User has the 'ADMIN' role (by code)
    IF EXISTS (
        SELECT 1
        FROM UserRoles ur
        JOIN Roles r ON ur.RoleId = r.Id
        WHERE ur.UserId = @CurrentUserId AND r.Code = 'ADMIN'
    )
    BEGIN
        SET @IsAdmin = 1;
    END

    SELECT @CurrentUserDesignationLevel = ISNULL(d.Level, 0)
    FROM Users u
    LEFT JOIN Designations d ON u.DesignationId = d.Id
    WHERE u.Id = @CurrentUserId AND u.IsDeleted = 0;

    SELECT @CurrentUserTopPriority = ISNULL(MIN(r.Priority), 2147483647)
    FROM UserRoles ur
    JOIN Roles r ON ur.RoleId = r.Id
    WHERE ur.UserId = @CurrentUserId AND r.IsDeleted = 0;

    -- 8. Check Designation Level Hierarchy (Bypassed for ADMIN)
    IF @IsAdmin = 0 AND @InvitedDesignationLevel >= @CurrentUserDesignationLevel
    BEGIN
        SET @ErrorCode = 6;
        RETURN;
    END

    -- 9. Check Role Authority Hierarchy by Priority (Bypassed for ADMIN).
    --    Lower Priority = higher authority. The invitee must be strictly lower
    --    authority than the inviter, i.e. invitee's top priority number must be
    --    greater than the inviter's.
    IF @IsAdmin = 0 AND @InvitedTopPriority <= @CurrentUserTopPriority
    BEGIN
        SET @ErrorCode = 8;
        RETURN;
    END

    BEGIN TRANSACTION;

    BEGIN TRY
        -- Generate New User Guid
        SET @NewUserId = NEWID();

        -- Create User. The inviter becomes the new user's direct manager (reports-to).
        INSERT INTO Users (Id, EmployeeCode, FirstName, LastName, Email, Username, PasswordHash, PhoneNumber, DepartmentId, DesignationId, ManagerId, IsActive, IsFirstTimeLogin, IsDeleted, CreatedBy, CreatedAt)
        VALUES (@NewUserId, @EmployeeCode, @FirstName, @LastName, @Email, @Username, @PasswordHash, @PhoneNumber, @DepartmentId, @DesignationId, @CurrentUserId, 1, 1, 0, @CreatedBy, GETUTCDATE());

        -- Assign Roles (if any roles are passed in the JSON array)
        IF @RoleIdsJson IS NOT NULL AND ISJSON(@RoleIdsJson) > 0
        BEGIN
            INSERT INTO UserRoles (UserId, RoleId)
            SELECT @NewUserId, CAST(value AS UNIQUEIDENTIFIER)
            FROM OPENJSON(@RoleIdsJson);
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
    }
}
