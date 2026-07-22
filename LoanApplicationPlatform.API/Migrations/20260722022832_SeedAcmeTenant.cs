using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LoanApplicationPlatform.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedAcmeTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "Name" },
                values: new object[] { 2, "Acme Finance" });

            migrationBuilder.InsertData(
                table: "Treasury",
                columns: new[] { "Id", "Balance", "TenantId" },
                values: new object[] { 2, 500000m, 2 });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "PasswordHash", "Role", "TenantId", "Username" },
                values: new object[,]
                {
                    { 2, "$2a$11$3ieT9rszDmCDFAmvST.IE.CBESY005xlEuNWBhleQUQNlA2kKHMV.", "Admin", 2, "acme_admin" },
                    { 3, "$2a$11$3ieT9rszDmCDFAmvST.IE.CBESY005xlEuNWBhleQUQNlA2kKHMV.", "Applicant", 2, "acme_applicant" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Treasury",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
