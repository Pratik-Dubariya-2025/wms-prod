using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeInviteUserHierarchyTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
    @ManagerId             UNIQUEIDENTIFIER = NULL,
    @ReportingOfficerId    UNIQUEIDENTIFIER = NULL,
    @NewUserId             UNIQUEIDENTIFIER OUTPUT,
    @ErrorCode             INT OUTPUT
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

    DECLARE @InvitedLevel INT;
    SELECT @InvitedLevel = d.Level
    FROM   Designations d
    WHERE  d.Id = @DesignationId AND d.IsDeleted = 0;

    IF @InvitedLevel IS NULL
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
        WHERE ur.UserId = @CurrentUserId AND r.Code = 'ADMIN'
    )
        SET @IsAdmin = 1;

    SELECT @CurrentUserDesignationLevel = ISNULL(d.Level, 0)
    FROM Users u
    LEFT JOIN Designations d ON u.DesignationId = d.Id
    WHERE u.Id = @CurrentUserId AND u.IsDeleted = 0;

    SELECT @CurrentUserTopPriority = ISNULL(MIN(r.Priority), 2147483647)
    FROM UserRoles ur
    JOIN Roles r ON ur.RoleId = r.Id
    WHERE ur.UserId = @CurrentUserId AND r.IsDeleted = 0;

    IF @IsAdmin = 0 AND @InvitedLevel >= @CurrentUserDesignationLevel
    BEGIN SET @ErrorCode = 6; RETURN; END

    IF @IsAdmin = 0 AND @InvitedTopPriority <= @CurrentUserTopPriority
    BEGIN SET @ErrorCode = 8; RETURN; END

    DECLARE @TopLevel INT;
    SELECT @TopLevel = MAX(d.Level)
    FROM   Users u
    JOIN   Designations d ON d.Id = u.DesignationId
    WHERE  u.DepartmentId = @DepartmentId AND u.IsDeleted = 0 AND u.IsActive = 1;

    DECLARE @LeadLevel INT = @TopLevel - 1;

    DECLARE @IsTopTier  BIT = CASE WHEN @TopLevel IS NULL OR @InvitedLevel >= @TopLevel THEN 1 ELSE 0 END;
    DECLARE @IsLeadTier BIT = CASE WHEN @IsTopTier = 0 AND @InvitedLevel = @LeadLevel THEN 1 ELSE 0 END;
    DECLARE @IsBelowLead BIT = CASE WHEN @IsTopTier = 0 AND @InvitedLevel < @LeadLevel THEN 1 ELSE 0 END;

    IF @IsTopTier = 0
    BEGIN
        IF @ManagerId IS NULL BEGIN SET @ErrorCode = 9; RETURN; END

        DECLARE @ManagerLevel INT;
        DECLARE @ManagerDeptId UNIQUEIDENTIFIER;
        SELECT @ManagerLevel  = d.Level,
               @ManagerDeptId = u.DepartmentId
        FROM   Users u
        JOIN   Designations d ON d.Id = u.DesignationId
        WHERE  u.Id = @ManagerId AND u.IsDeleted = 0 AND u.IsActive = 1;

        IF @ManagerDeptId IS NULL
            OR @ManagerDeptId <> @DepartmentId
            OR @ManagerLevel <> @TopLevel
            OR @ManagerLevel <= @InvitedLevel
        BEGIN SET @ErrorCode = 9; RETURN; END
    END

    DECLARE @ROLevel INT, @RODeptId UNIQUEIDENTIFIER, @ROTeamId UNIQUEIDENTIFIER;

    IF @IsBelowLead = 1 AND @ReportingOfficerId IS NULL
    BEGIN SET @ErrorCode = 11; RETURN; END

    IF @ReportingOfficerId IS NOT NULL
    BEGIN
        SELECT @ROLevel    = d.Level,
               @RODeptId   = u.DepartmentId,
               @ROTeamId   = u.TeamId
        FROM   Users u
        JOIN   Designations d ON d.Id = u.DesignationId
        WHERE  u.Id = @ReportingOfficerId AND u.IsDeleted = 0 AND u.IsActive = 1;

        IF @RODeptId IS NULL
            OR @RODeptId <> @DepartmentId
            OR @ROLevel <= @InvitedLevel
            OR (@ManagerId IS NOT NULL AND @ROLevel > @TopLevel)
        BEGIN SET @ErrorCode = 10; RETURN; END
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        SET @NewUserId = NEWID();

        DECLARE @DerivedTeamId UNIQUEIDENTIFIER = NULL;

        IF @IsBelowLead = 1
        BEGIN
            IF @ROTeamId IS NULL
            BEGIN
                DECLARE @RONameTeam NVARCHAR(200);
                SELECT @RONameTeam = u.FirstName + ' ' + u.LastName + '''s Team'
                FROM   Users u WHERE u.Id = @ReportingOfficerId;

                DECLARE @RoTeamNewId UNIQUEIDENTIFIER = NEWID();
                INSERT INTO Teams (Id, Name, DepartmentId, TeamLeadId, IsDeleted, CreatedBy, CreatedAt)
                VALUES (@RoTeamNewId, @RONameTeam, @DepartmentId, @ReportingOfficerId, 0, @CreatedBy, GETUTCDATE());

                UPDATE Users SET TeamId = @RoTeamNewId WHERE Id = @ReportingOfficerId;
                SET @DerivedTeamId = @RoTeamNewId;
            END
            ELSE
                SET @DerivedTeamId = @ROTeamId;
        END

        INSERT INTO Users (
            Id, EmployeeCode, FirstName, LastName, Email, Username,
            PasswordHash, PhoneNumber, DepartmentId, DesignationId,
            ManagerId, ReportingOfficerId, TeamId,
            IsActive, IsFirstTimeLogin, IsDeleted, CreatedBy, CreatedAt
        )
        VALUES (
            @NewUserId, @EmployeeCode, @FirstName, @LastName, @Email,
            @Username, @PasswordHash, @PhoneNumber, @DepartmentId, @DesignationId,
            @ManagerId, @ReportingOfficerId, @DerivedTeamId,
            1, 1, 0, @CreatedBy, GETUTCDATE()
        );

        IF @IsLeadTier = 1
        BEGIN
            DECLARE @LeadTeamId UNIQUEIDENTIFIER = NEWID();
            INSERT INTO Teams (Id, Name, DepartmentId, TeamLeadId, IsDeleted, CreatedBy, CreatedAt)
            VALUES (@LeadTeamId, @FirstName + ' ' + @LastName + '''s Team',
                    @DepartmentId, @NewUserId, 0, @CreatedBy, GETUTCDATE());

            UPDATE Users SET TeamId = @LeadTeamId WHERE Id = @NewUserId;
        END

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
            // Revert to the previous procedure shape (pre-team-in-SP, optional RO).
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
    @ManagerId             UNIQUEIDENTIFIER = NULL,
    @ReportingOfficerId    UNIQUEIDENTIFIER = NULL,
    @NewUserId             UNIQUEIDENTIFIER OUTPUT,
    @ErrorCode             INT OUTPUT
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

    DECLARE @InvitedLevel INT;
    SELECT @InvitedLevel = d.Level FROM Designations d WHERE d.Id = @DesignationId AND d.IsDeleted = 0;
    IF @InvitedLevel IS NULL BEGIN SET @ErrorCode = 5; RETURN; END

    DECLARE @InvitedTopPriority INT = 2147483647;
    DECLARE @PassedRoleCount INT = 0;
    DECLARE @ValidRoleCount INT = 0;
    SELECT @PassedRoleCount = COUNT(*) FROM OPENJSON(@RoleIdsJson);
    SELECT @ValidRoleCount = COUNT(*), @InvitedTopPriority = ISNULL(MIN(r.Priority), 2147483647)
    FROM Roles r WHERE r.IsDeleted = 0
      AND r.Id IN (SELECT CAST(value AS UNIQUEIDENTIFIER) FROM OPENJSON(@RoleIdsJson));
    IF @ValidRoleCount <> @PassedRoleCount OR @PassedRoleCount = 0 BEGIN SET @ErrorCode = 7; RETURN; END

    DECLARE @CurrentUserDesignationLevel INT = 0;
    DECLARE @CurrentUserTopPriority INT = 2147483647;
    DECLARE @IsAdmin BIT = 0;
    IF EXISTS (SELECT 1 FROM UserRoles ur JOIN Roles r ON ur.RoleId = r.Id WHERE ur.UserId = @CurrentUserId AND r.Code = 'ADMIN')
        SET @IsAdmin = 1;
    SELECT @CurrentUserDesignationLevel = ISNULL(d.Level, 0) FROM Users u LEFT JOIN Designations d ON u.DesignationId = d.Id WHERE u.Id = @CurrentUserId AND u.IsDeleted = 0;
    SELECT @CurrentUserTopPriority = ISNULL(MIN(r.Priority), 2147483647) FROM UserRoles ur JOIN Roles r ON ur.RoleId = r.Id WHERE ur.UserId = @CurrentUserId AND r.IsDeleted = 0;

    IF @IsAdmin = 0 AND @InvitedLevel >= @CurrentUserDesignationLevel BEGIN SET @ErrorCode = 6; RETURN; END
    IF @IsAdmin = 0 AND @InvitedTopPriority <= @CurrentUserTopPriority BEGIN SET @ErrorCode = 8; RETURN; END

    IF @InvitedLevel < 5
    BEGIN
        DECLARE @MaxLevelInDept INT;
        SELECT @MaxLevelInDept = MAX(d.Level) FROM Users u JOIN Designations d ON d.Id = u.DesignationId
        WHERE u.DepartmentId = @DepartmentId AND u.IsDeleted = 0 AND u.IsActive = 1;
        DECLARE @ManagerLevel INT; DECLARE @ManagerDeptId UNIQUEIDENTIFIER;
        SELECT @ManagerLevel = d.Level, @ManagerDeptId = u.DepartmentId FROM Users u JOIN Designations d ON d.Id = u.DesignationId
        WHERE u.Id = @ManagerId AND u.IsDeleted = 0 AND u.IsActive = 1;
        IF @ManagerDeptId IS NULL OR @ManagerDeptId <> @DepartmentId OR @ManagerLevel <> @MaxLevelInDept OR @ManagerLevel <= @InvitedLevel
        BEGIN SET @ErrorCode = 9; RETURN; END
    END

    DECLARE @DerivedTeamId UNIQUEIDENTIFIER = NULL; DECLARE @ROLevel INT;
    IF @ReportingOfficerId IS NOT NULL
    BEGIN
        DECLARE @RODeptId UNIQUEIDENTIFIER;
        SELECT @ROLevel = d.Level, @RODeptId = u.DepartmentId, @DerivedTeamId = u.TeamId FROM Users u JOIN Designations d ON d.Id = u.DesignationId
        WHERE u.Id = @ReportingOfficerId AND u.IsDeleted = 0 AND u.IsActive = 1;
        IF @RODeptId IS NULL OR @RODeptId <> @DepartmentId OR @ROLevel <= @InvitedLevel
            OR (@ManagerId IS NOT NULL AND @ROLevel > (SELECT d2.Level FROM Users u2 JOIN Designations d2 ON d2.Id = u2.DesignationId WHERE u2.Id = @ManagerId))
        BEGIN SET @ErrorCode = 10; RETURN; END
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        SET @NewUserId = NEWID();
        INSERT INTO Users (Id, EmployeeCode, FirstName, LastName, Email, Username, PasswordHash, PhoneNumber, DepartmentId, DesignationId, ManagerId, ReportingOfficerId, TeamId, IsActive, IsFirstTimeLogin, IsDeleted, CreatedBy, CreatedAt)
        VALUES (@NewUserId, @EmployeeCode, @FirstName, @LastName, @Email, @Username, @PasswordHash, @PhoneNumber, @DepartmentId, @DesignationId, @ManagerId, @ReportingOfficerId, @DerivedTeamId, 1, 1, 0, @CreatedBy, GETUTCDATE());
        IF @RoleIdsJson IS NOT NULL AND ISJSON(@RoleIdsJson) > 0
        BEGIN
            INSERT INTO UserRoles (UserId, RoleId) SELECT @NewUserId, CAST(value AS UNIQUEIDENTIFIER) FROM OPENJSON(@RoleIdsJson);
        END
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION; SET @ErrorCode = 99; THROW;
    END CATCH
END
");
        }
    }
}
