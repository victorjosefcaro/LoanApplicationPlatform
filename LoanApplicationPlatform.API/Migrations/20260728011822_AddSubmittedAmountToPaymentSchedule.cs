using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanApplicationPlatform.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmittedAmountToPaymentSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SubmittedAmount",
                table: "PaymentSchedules",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubmittedAmount",
                table: "PaymentSchedules");
        }
    }
}
