using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Persistence.Migrations
{
    public partial class AddInviteUserStoredProcedure : Migration
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_InviteUser;");
        }
    }
}
