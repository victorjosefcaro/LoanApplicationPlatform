using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanApplicationPlatform.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMultitenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "TreasuryTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Treasury",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "PaymentSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "LoanApplications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Tenants",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Default Lending Co" });

            migrationBuilder.UpdateData(
                table: "Treasury",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "TenantId",
                value: 1);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId",
                table: "Users",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_TenantId",
                table: "TreasuryTransactions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Treasury_TenantId",
                table: "Treasury",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSchedules_TenantId",
                table: "PaymentSchedules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanApplications_TenantId",
                table: "LoanApplications",
                column: "TenantId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanApplications_Tenants_TenantId",
                table: "LoanApplications",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentSchedules_Tenants_TenantId",
                table: "PaymentSchedules",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Treasury_Tenants_TenantId",
                table: "Treasury",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TreasuryTransactions_Tenants_TenantId",
                table: "TreasuryTransactions",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanApplications_Tenants_TenantId",
                table: "LoanApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentSchedules_Tenants_TenantId",
                table: "PaymentSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_Treasury_Tenants_TenantId",
                table: "Treasury");

            migrationBuilder.DropForeignKey(
                name: "FK_TreasuryTransactions_Tenants_TenantId",
                table: "TreasuryTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Tenants_TenantId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_TreasuryTransactions_TenantId",
                table: "TreasuryTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Treasury_TenantId",
                table: "Treasury");

            migrationBuilder.DropIndex(
                name: "IX_PaymentSchedules_TenantId",
                table: "PaymentSchedules");

            migrationBuilder.DropIndex(
                name: "IX_LoanApplications_TenantId",
                table: "LoanApplications");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "TreasuryTransactions");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Treasury");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PaymentSchedules");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "LoanApplications");
        }
    }
}
