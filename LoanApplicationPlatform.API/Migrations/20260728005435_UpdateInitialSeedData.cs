using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LoanApplicationPlatform.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInitialSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Tenant 1");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Tenant 2");

            migrationBuilder.UpdateData(
                table: "Treasury",
                keyColumn: "Id",
                keyValue: 2,
                column: "Balance",
                value: 2000000m);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Username",
                value: "t1_admin");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Role", "TenantId", "Username" },
                values: new object[] { "Applicant", 1, "t1_applicant" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Role", "TenantId", "Username" },
                values: new object[] { "Reviewer", 1, "t1_reviewer" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "PasswordHash", "Role", "TenantId", "Username" },
                values: new object[,]
                {
                    { 4, "$2a$11$3ieT9rszDmCDFAmvST.IE.CBESY005xlEuNWBhleQUQNlA2kKHMV.", "Approver", 1, "t1_approver" },
                    { 5, "$2a$11$3ieT9rszDmCDFAmvST.IE.CBESY005xlEuNWBhleQUQNlA2kKHMV.", "Admin", 2, "t2_admin" },
                    { 6, "$2a$11$3ieT9rszDmCDFAmvST.IE.CBESY005xlEuNWBhleQUQNlA2kKHMV.", "Applicant", 2, "t2_applicant" },
                    { 7, "$2a$11$3ieT9rszDmCDFAmvST.IE.CBESY005xlEuNWBhleQUQNlA2kKHMV.", "Reviewer", 2, "t2_reviewer" },
                    { 8, "$2a$11$3ieT9rszDmCDFAmvST.IE.CBESY005xlEuNWBhleQUQNlA2kKHMV.", "Approver", 2, "t2_approver" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Default Lending Co");

            migrationBuilder.UpdateData(
                table: "Tenants",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Acme Finance");

            migrationBuilder.UpdateData(
                table: "Treasury",
                keyColumn: "Id",
                keyValue: 2,
                column: "Balance",
                value: 500000m);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Username",
                value: "admin");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Role", "TenantId", "Username" },
                values: new object[] { "Admin", 2, "acme_admin" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Role", "TenantId", "Username" },
                values: new object[] { "Applicant", 2, "acme_applicant" });
        }
    }
}
