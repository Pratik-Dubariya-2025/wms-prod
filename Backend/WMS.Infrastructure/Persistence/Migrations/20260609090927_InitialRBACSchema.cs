using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialRBACSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    IsSystemRole = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Designations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Level = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Designations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Designations_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    CreatedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DesignationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Users_Designations_DesignationId",
                        column: x => x.DesignationId,
                        principalTable: "Designations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TokenHash = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedBy = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsActive", "ModifiedAt", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("50000000-0000-0000-0000-000000000001"), "IT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Software development, infrastructure, and technical support", true, null, null, "Information Technology" },
                    { new Guid("50000000-0000-0000-0000-000000000002"), "HR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Employee management, recruitment, and compliance", true, null, null, "Human Resources" },
                    { new Guid("50000000-0000-0000-0000-000000000003"), "FIN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Accounting, budgeting, and financial reporting", true, null, null, "Finance" },
                    { new Guid("50000000-0000-0000-0000-000000000004"), "OPS", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Day-to-day business operations and logistics", true, null, null, "Operations" }
                });

            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "DisplayOrder", "IsActive", "ModifiedAt", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-e5f6-4a5b-8c7d-9e0f1a2b3c4d"), "USER_MGMT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Manage users, roles, and permissions", 1, true, null, null, "User Management" },
                    { new Guid("b2c3d4e5-f6a7-4b5c-9d8e-0f1a2b3c4d5e"), "DEPT_MGMT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Manage departments and designations", 2, true, null, null, "Department Management" },
                    { new Guid("c3d4e5f6-a7b8-4c5d-0e9f-1a2b3c4d5e6f"), "ROLE_MGMT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Manage roles and assign permissions", 3, true, null, null, "Role Management" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "IsSystemRole", "ModifiedAt", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("7c1d6d5f-f9d7-44c9-b7e2-5282f3f0ab09"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Standard employee access", true, null, null, "EMPLOYEE" },
                    { new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Full system access", true, null, null, "ADMIN" },
                    { new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "HR department management access", true, null, null, "HR_MANAGER" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "ModifiedAt", "ModifiedBy", "Name" },
                values: new object[] { new Guid("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Team lead access with approval rights", null, null, "TEAM_LEAD" });

            migrationBuilder.InsertData(
                table: "Designations",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "Description", "IsActive", "Level", "ModifiedAt", "ModifiedBy", "Name" },
                values: new object[,]
                {
                    { new Guid("60000000-0000-0000-0000-000000000001"), "JR_DEV", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("50000000-0000-0000-0000-000000000001"), null, true, 1, null, null, "Junior Developer" },
                    { new Guid("60000000-0000-0000-0000-000000000002"), "TSE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("50000000-0000-0000-0000-000000000001"), null, true, 2, null, null, "Technical Support Engineer" },
                    { new Guid("60000000-0000-0000-0000-000000000003"), "SR_DEV", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("50000000-0000-0000-0000-000000000001"), null, true, 3, null, null, "Senior Developer" },
                    { new Guid("60000000-0000-0000-0000-000000000004"), "TECH_LEAD", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("50000000-0000-0000-0000-000000000001"), null, true, 4, null, null, "Tech Lead" },
                    { new Guid("60000000-0000-0000-0000-000000000005"), "HR_EXEC", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("50000000-0000-0000-0000-000000000002"), null, true, 1, null, null, "HR Executive" },
                    { new Guid("60000000-0000-0000-0000-000000000006"), "HR_MGR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("50000000-0000-0000-0000-000000000002"), null, true, 3, null, null, "HR Manager" },
                    { new Guid("60000000-0000-0000-0000-000000000007"), "ACCT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("50000000-0000-0000-0000-000000000003"), null, true, 1, null, null, "Accountant" },
                    { new Guid("60000000-0000-0000-0000-000000000008"), "FIN_MGR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("50000000-0000-0000-0000-000000000003"), null, true, 3, null, null, "Finance Manager" }
                });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Action", "Code", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Description", "ModifiedAt", "ModifiedBy", "ModuleId" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "Create", "user.create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Create new users", null, null, new Guid("a1b2c3d4-e5f6-4a5b-8c7d-9e0f1a2b3c4d") },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "Read", "user.read", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "View user details", null, null, new Guid("a1b2c3d4-e5f6-4a5b-8c7d-9e0f1a2b3c4d") },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "Update", "user.update", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Edit user details", null, null, new Guid("a1b2c3d4-e5f6-4a5b-8c7d-9e0f1a2b3c4d") },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "Delete", "user.delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Delete users", null, null, new Guid("a1b2c3d4-e5f6-4a5b-8c7d-9e0f1a2b3c4d") },
                    { new Guid("20000000-0000-0000-0000-000000000001"), "Create", "department.create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Create departments", null, null, new Guid("b2c3d4e5-f6a7-4b5c-9d8e-0f1a2b3c4d5e") },
                    { new Guid("20000000-0000-0000-0000-000000000002"), "Read", "department.read", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "View departments", null, null, new Guid("b2c3d4e5-f6a7-4b5c-9d8e-0f1a2b3c4d5e") },
                    { new Guid("20000000-0000-0000-0000-000000000003"), "Update", "department.update", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Edit departments", null, null, new Guid("b2c3d4e5-f6a7-4b5c-9d8e-0f1a2b3c4d5e") },
                    { new Guid("20000000-0000-0000-0000-000000000004"), "Delete", "department.delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Delete departments", null, null, new Guid("b2c3d4e5-f6a7-4b5c-9d8e-0f1a2b3c4d5e") },
                    { new Guid("30000000-0000-0000-0000-000000000001"), "Create", "role.create", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Create roles", null, null, new Guid("c3d4e5f6-a7b8-4c5d-0e9f-1a2b3c4d5e6f") },
                    { new Guid("30000000-0000-0000-0000-000000000002"), "Read", "role.read", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "View roles", null, null, new Guid("c3d4e5f6-a7b8-4c5d-0e9f-1a2b3c4d5e6f") },
                    { new Guid("30000000-0000-0000-0000-000000000003"), "Update", "role.update", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Edit roles", null, null, new Guid("c3d4e5f6-a7b8-4c5d-0e9f-1a2b3c4d5e6f") },
                    { new Guid("30000000-0000-0000-0000-000000000004"), "Delete", "role.delete", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "Delete roles", null, null, new Guid("c3d4e5f6-a7b8-4c5d-0e9f-1a2b3c4d5e6f") }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000002"), new Guid("7c1d6d5f-f9d7-44c9-b7e2-5282f3f0ab09") },
                    { new Guid("20000000-0000-0000-0000-000000000002"), new Guid("7c1d6d5f-f9d7-44c9-b7e2-5282f3f0ab09") },
                    { new Guid("30000000-0000-0000-0000-000000000002"), new Guid("7c1d6d5f-f9d7-44c9-b7e2-5282f3f0ab09") },
                    { new Guid("10000000-0000-0000-0000-000000000001"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") },
                    { new Guid("10000000-0000-0000-0000-000000000002"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") },
                    { new Guid("10000000-0000-0000-0000-000000000003"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") },
                    { new Guid("10000000-0000-0000-0000-000000000004"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") },
                    { new Guid("20000000-0000-0000-0000-000000000001"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") },
                    { new Guid("20000000-0000-0000-0000-000000000002"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") },
                    { new Guid("20000000-0000-0000-0000-000000000003"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") },
                    { new Guid("20000000-0000-0000-0000-000000000004"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") },
                    { new Guid("30000000-0000-0000-0000-000000000001"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") },
                    { new Guid("30000000-0000-0000-0000-000000000002"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") },
                    { new Guid("30000000-0000-0000-0000-000000000003"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") },
                    { new Guid("30000000-0000-0000-0000-000000000004"), new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98") },
                    { new Guid("10000000-0000-0000-0000-000000000001"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") },
                    { new Guid("10000000-0000-0000-0000-000000000002"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") },
                    { new Guid("10000000-0000-0000-0000-000000000003"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") },
                    { new Guid("10000000-0000-0000-0000-000000000004"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") },
                    { new Guid("20000000-0000-0000-0000-000000000001"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") },
                    { new Guid("20000000-0000-0000-0000-000000000002"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") },
                    { new Guid("20000000-0000-0000-0000-000000000003"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") },
                    { new Guid("20000000-0000-0000-0000-000000000004"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") },
                    { new Guid("30000000-0000-0000-0000-000000000001"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") },
                    { new Guid("30000000-0000-0000-0000-000000000002"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") },
                    { new Guid("30000000-0000-0000-0000-000000000003"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") },
                    { new Guid("30000000-0000-0000-0000-000000000004"), new Guid("d4e5f6a7-b8c9-4d5e-1f0a-2b3c4d5e6f7a") },
                    { new Guid("10000000-0000-0000-0000-000000000002"), new Guid("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b") },
                    { new Guid("10000000-0000-0000-0000-000000000003"), new Guid("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b") },
                    { new Guid("20000000-0000-0000-0000-000000000002"), new Guid("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b") },
                    { new Guid("30000000-0000-0000-0000-000000000002"), new Guid("e5f6a7b8-c9d0-4e5f-2a1b-3c4d5e6f7a8b") }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DepartmentId", "DesignationId", "Email", "EmployeeCode", "FirstName", "IsActive", "LastName", "ModifiedAt", "ModifiedBy", "PasswordHash", "PhoneNumber", "Username" },
                values: new object[,]
                {
                    { new Guid("70000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("50000000-0000-0000-0000-000000000001"), new Guid("60000000-0000-0000-0000-000000000004"), "admin@wms.com", "EMP-001", "System", true, "Admin", null, null, "$2a$11$fxpkGie8ewg0L3TFvIEOY.Zh6YHwpqlOcScYRjkZKjPGp/lFCbiZW", "1234567890", "admin" },
                    { new Guid("70000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, new Guid("50000000-0000-0000-0000-000000000001"), new Guid("60000000-0000-0000-0000-000000000002"), "pratik@wms.com", "EMP-002", "Pratik", true, "Dubariya", null, null, "$2a$11$fxpkGie8ewg0L3TFvIEOY.Zh6YHwpqlOcScYRjkZKjPGp/lFCbiZW", "9876543210", "pratik" }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { new Guid("7c1d6d5f-f9d7-44c9-b7e2-5282f3f0ab09"), new Guid("70000000-0000-0000-0000-000000000001") },
                    { new Guid("9b0c5c4e-f8c6-43b8-a6d1-4171e2ef9a98"), new Guid("70000000-0000-0000-0000-000000000001") },
                    { new Guid("7c1d6d5f-f9d7-44c9-b7e2-5282f3f0ab09"), new Guid("70000000-0000-0000-0000-000000000002") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Code",
                table: "Departments",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name",
                table: "Departments",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Designations_Code",
                table: "Designations",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Designations_DepartmentId",
                table: "Designations",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Code",
                table: "Modules",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_Name",
                table: "Modules",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ModuleId",
                table: "Permissions",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DesignationId",
                table: "Users",
                column: "DesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeCode",
                table: "Users",
                column: "EmployeeCode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropTable(
                name: "Designations");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
