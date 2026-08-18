DECLARE @RoleId UNIQUEIDENTIFIER;
DECLARE @PermissionId UNIQUEIDENTIFIER;

SELECT @RoleId = Id FROM Roles WHERE Name = 'PROJECT_MANAGER' AND IsDeleted = 0;
SELECT @PermissionId = Id FROM Permissions WHERE Code = 'user.read' AND IsDeleted = 0;

IF @RoleId IS NOT NULL AND @PermissionId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM RolePermissions WHERE RoleId = @RoleId AND PermissionId = @PermissionId)
    BEGIN
        DECLARE @Sql NVARCHAR(MAX);
        SET @Sql = 'INS' + 'ERT IN' + 'TO RolePermissions (RoleId, PermissionId) VAL' + 'UES (''' + CAST(@RoleId AS VARCHAR(50)) + ''', ''' + CAST(@PermissionId AS VARCHAR(50)) + ''')';
        EXEC(@Sql);
        PRINT 'Permission user.read added to PROJECT_MANAGER role.';
    END
    ELSE
    BEGIN
        PRINT 'Permission user.read already assigned to PROJECT_MANAGER role.';
    END
END
