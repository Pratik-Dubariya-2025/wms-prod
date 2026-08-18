using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateITDesignations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Designations",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000001"),
                columns: new[] { "Code", "Name" },
                values: new object[] { "JR_SE", "Junior Software Engineer" });

            migrationBuilder.UpdateData(
                table: "Designations",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000003"),
                columns: new[] { "Code", "Name" },
                values: new object[] { "SR_SE", "Senior Software Engineer" });

            migrationBuilder.InsertData(
                table: "Designations",
                columns: new[] { "Id", "DepartmentId", "Name", "Code", "Level", "IsActive", "CreatedAt" },
                values: new object[] { new Guid("60000000-0000-0000-0000-000000000009"), new Guid("50000000-0000-0000-0000-000000000001"), "Software Engineer", "SE", 2, true, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Designations",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000009"));

            migrationBuilder.UpdateData(
                table: "Designations",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000001"),
                columns: new[] { "Code", "Name" },
                values: new object[] { "JR_DEV", "Junior Developer" });

            migrationBuilder.UpdateData(
                table: "Designations",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000003"),
                columns: new[] { "Code", "Name" },
                values: new object[] { "SR_DEV", "Senior Developer" });
        }
    }
}
