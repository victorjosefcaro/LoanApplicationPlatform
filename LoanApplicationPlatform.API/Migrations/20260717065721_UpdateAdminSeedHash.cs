using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanApplicationPlatform.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminSeedHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$3ieT9rszDmCDFAmvST.IE.CBESY005xlEuNWBhleQUQNlA2kKHMV.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "dummyhash");
        }
    }
}
