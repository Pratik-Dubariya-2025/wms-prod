SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

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
    @ManagerId             UNIQUEIDENTIFIER = NULL,    -- department head; required below top tier
    @ReportingOfficerId    UNIQUEIDENTIFIER = NULL,    -- Team Lead (or Manager when no TL); required below lead tier
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
    BEGIN SET @ErrorCode = 7; RETURN; END

    -- 7. Current user's designation level + highest authority role priority + admin flag.
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

    -- 8. Designation level hierarchy (bypassed for ADMIN).
    IF @IsAdmin = 0 AND @InvitedLevel >= @CurrentUserDesignationLevel
    BEGIN SET @ErrorCode = 6; RETURN; END

    -- 9. Role authority hierarchy by priority (bypassed for ADMIN).
    IF @IsAdmin = 0 AND @InvitedTopPriority <= @CurrentUserTopPriority
    BEGIN SET @ErrorCode = 8; RETURN; END

    -- 10. Determine the department tiers.
    --     TopLevel  = department head / manager (PM) tier.
    --     LeadLevel = team lead (TL) tier (TopLevel - 1).
    DECLARE @TopLevel INT;
    SELECT @TopLevel = MAX(d.Level)
    FROM   Users u
    JOIN   Designations d ON d.Id = u.DesignationId
    WHERE  u.DepartmentId = @DepartmentId AND u.IsDeleted = 0 AND u.IsActive = 1;

    DECLARE @LeadLevel INT = @TopLevel - 1;

    DECLARE @IsTopTier  BIT = CASE WHEN @TopLevel IS NULL OR @InvitedLevel >= @TopLevel THEN 1 ELSE 0 END;
    DECLARE @IsLeadTier BIT = CASE WHEN @IsTopTier = 0 AND @InvitedLevel = @LeadLevel THEN 1 ELSE 0 END;
    DECLARE @IsBelowLead BIT = CASE WHEN @IsTopTier = 0 AND @InvitedLevel < @LeadLevel THEN 1 ELSE 0 END;

    -- 11. Validate Manager (required for every tier below the top / department head).
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
            OR @ManagerLevel <> @TopLevel        -- manager must be the department head
            OR @ManagerLevel <= @InvitedLevel
        BEGIN SET @ErrorCode = 9; RETURN; END
    END

    -- 12. Validate Reporting Officer.
    --     Required for the below-lead (developer) tier so every developer lands in a team.
    --     May be a Team Lead, or the Manager themselves when no Team Lead exists.
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
            OR @ROLevel <= @InvitedLevel          -- RO must rank strictly above the invited user
            OR (@ManagerId IS NOT NULL AND @ROLevel > @TopLevel)   -- RO cannot outrank the manager tier
        BEGIN SET @ErrorCode = 10; RETURN; END
    END

    -- 13. Persist atomically: user, derived/created team, and role assignments.
    BEGIN TRANSACTION;
    BEGIN TRY
        SET @NewUserId = NEWID();

        DECLARE @DerivedTeamId UNIQUEIDENTIFIER = NULL;

        -- Developer tier: join the reporting officer's team. If the officer has no
        -- team (e.g. the officer is the Manager standing in for a missing TL),
        -- create one led by them so the developer is never team-less.
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

        -- Lead tier (Tech Lead): auto-create their own team and link them to it.
        IF @IsLeadTier = 1
        BEGIN
            DECLARE @LeadTeamId UNIQUEIDENTIFIER = NEWID();
            INSERT INTO Teams (Id, Name, DepartmentId, TeamLeadId, IsDeleted, CreatedBy, CreatedAt)
            VALUES (@LeadTeamId, @FirstName + ' ' + @LastName + '''s Team',
                    @DepartmentId, @NewUserId, 0, @CreatedBy, GETUTCDATE());

            UPDATE Users SET TeamId = @LeadTeamId WHERE Id = @NewUserId;
        END

        -- Role assignment
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
GO
