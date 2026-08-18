using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Persistence.Migrations
{
    public partial class UpdateInviteUserStoredProcedureWithHierarchy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

    -- 6. Validate Roles exist and find max rank
    DECLARE @InvitedMaxRoleRank INT = 0;
    DECLARE @PassedRoleCount INT = 0;
    DECLARE @ValidRoleCount INT = 0;

    SELECT @PassedRoleCount = COUNT(*) FROM OPENJSON(@RoleIdsJson);

    SELECT @ValidRoleCount = COUNT(*),
           @InvitedMaxRoleRank = ISNULL(MAX(p.PermCount), 0)
    FROM Roles r
    OUTER APPLY (
        SELECT COUNT(*) AS PermCount
        FROM RolePermissions rp
        WHERE rp.RoleId = r.Id
    ) p
    WHERE r.Id IN (
        SELECT CAST(value AS UNIQUEIDENTIFIER)
        FROM OPENJSON(@RoleIdsJson)
    );

    IF @ValidRoleCount <> @PassedRoleCount OR @PassedRoleCount = 0
    BEGIN
        SET @ErrorCode = 7;
        RETURN;
    END

    -- 7. Get Current User's Designation Level and Max Role Rank
    DECLARE @CurrentUserDesignationLevel INT = 0;
    DECLARE @CurrentUserMaxRoleRank INT = 0;
    DECLARE @IsAdmin BIT = 0;

    -- Check if Current User has the 'ADMIN' role
    IF EXISTS (
        SELECT 1 
        FROM UserRoles ur
        JOIN Roles r ON ur.RoleId = r.Id
        WHERE ur.UserId = @CurrentUserId AND r.Name = 'ADMIN'
    )
    BEGIN
        SET @IsAdmin = 1;
    END

    SELECT @CurrentUserDesignationLevel = ISNULL(d.Level, 0)
    FROM Users u
    LEFT JOIN Designations d ON u.DesignationId = d.Id
    WHERE u.Id = @CurrentUserId AND u.IsDeleted = 0;

    SELECT @CurrentUserMaxRoleRank = ISNULL(MAX(p.PermCount), 0)
    FROM UserRoles ur
    OUTER APPLY (
        SELECT COUNT(*) AS PermCount
        FROM RolePermissions rp
        WHERE rp.RoleId = ur.RoleId
    ) p
    WHERE ur.UserId = @CurrentUserId;

    -- 8. Check Designation Level Hierarchy (Bypassed for ADMIN)
    IF @IsAdmin = 0 AND @InvitedDesignationLevel >= @CurrentUserDesignationLevel
    BEGIN
        SET @ErrorCode = 6;
        RETURN;
    END

    -- 9. Check Role Rank Hierarchy (Bypassed for ADMIN)
    IF @IsAdmin = 0 AND @InvitedMaxRoleRank >= @CurrentUserMaxRoleRank
    BEGIN
        SET @ErrorCode = 8;
        RETURN;
    END

    BEGIN TRANSACTION;

    BEGIN TRY
        -- Generate New User Guid
        SET @NewUserId = NEWID();

        -- Create User
        INSERT INTO Users (Id, EmployeeCode, FirstName, LastName, Email, Username, PasswordHash, PhoneNumber, DepartmentId, DesignationId, IsActive, IsFirstTimeLogin, IsDeleted, CreatedBy, CreatedAt)
        VALUES (@NewUserId, @EmployeeCode, @FirstName, @LastName, @Email, @Username, @PasswordHash, @PhoneNumber, @DepartmentId, @DesignationId, 1, 1, 0, @CreatedBy, GETUTCDATE());

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Fall back to original sp_InviteUser without hierarchy check
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

    -- 5. Validate Designation exists
    IF NOT EXISTS (SELECT 1 FROM Designations WHERE Id = @DesignationId AND IsDeleted = 0)
    BEGIN
        SET @ErrorCode = 5;
        RETURN;
    END

    BEGIN TRANSACTION;

    BEGIN TRY
        -- Generate New User Guid
        SET @NewUserId = NEWID();

        -- Create User
        INSERT INTO Users (Id, EmployeeCode, FirstName, LastName, Email, Username, PasswordHash, PhoneNumber, DepartmentId, DesignationId, IsActive, IsFirstTimeLogin, IsDeleted, CreatedBy, CreatedAt)
        VALUES (@NewUserId, @EmployeeCode, @FirstName, @LastName, @Email, @Username, @PasswordHash, @PhoneNumber, @DepartmentId, @DesignationId, 1, 1, 0, @CreatedBy, GETUTCDATE());

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
